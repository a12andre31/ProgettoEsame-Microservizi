using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Pagamenti.Business.Abstraction;
using Pagamenti.Shared;
using Utility.Kafka.Abstractions.MessageHandlers;
using Utility.Kafka.Messages;
using Utility.Kafka.Constants;

namespace Pagamenti.Business.Kafka;

public class ConsumerHandler(
    ILogger<ConsumerHandler> logger,
    IServiceScopeFactory serviceScopeFactory) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken)
    {
        try
        {
            // Sbirciamo solo la parola "Operation" per decidere quale DTO usare
            using var doc = JsonDocument.Parse(message);
            var operation = doc.RootElement.GetProperty("Operation").GetString();

            using var scope = serviceScopeFactory.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            // Avanzamento SAGA (Addebito)
            if (operation == Operations.Insert)
            {
                var opMsg = JsonSerializer.Deserialize<OperationMessage<OrdineDaPagareDto>>(message);

                if (opMsg?.Dto != null)
                {
                    logger.LogInformation("Richiesta pagamento per Ordine {IdOrdine}. Importo: {Importo}", opMsg.Dto.IdOrdine, opMsg.Dto.Importo);
                    await business.ElaboraPagamentoAsync(opMsg.Dto.IdOrdine, opMsg.Dto.IdCliente, opMsg.Dto.Importo, cancellationToken);
                }
            }
            // Compensazione SAGA (Rimborso)
            else if (operation == Operations.Update)
            {
                var opMsg = JsonSerializer.Deserialize<OperationMessage<OrdineAnnullatoDto>>(message);

                // Rimborsa SOLO se c'è stato un timeout di rete sulla propria transazione
                if (opMsg?.Dto != null && opMsg.Dto.Stato == "Timeout_Pagamenti")
                {
                    logger.LogWarning("Rimborso di emergenza di {Importo} al cliente {IdCliente} per Timeout.", opMsg.Dto.PrezzoTotale, opMsg.Dto.IdCliente);
                    await business.RimborsaPagamentoAsync(opMsg.Dto.IdCliente, opMsg.Dto.PrezzoTotale, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore lettura messaggio Kafka in Pagamenti.");
        }
    }
}

public class ConsumerHandlerFactory(
    ILogger<ConsumerHandler> logger,
    IServiceScopeFactory serviceScopeFactory) : IMessageHandlerFactory<string, string>
{
    public IMessageHandler<string, string> Create(string topic, IServiceProvider serviceProvider)
    {
        return new ConsumerHandler(logger, serviceScopeFactory);
    }
}