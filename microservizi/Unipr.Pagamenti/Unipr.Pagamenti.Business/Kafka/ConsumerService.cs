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
            var opMsg = JsonSerializer.Deserialize<OperationMessage<OrdineDaPagareDto>>(message);

            // Solo se è una richiesta inserita da Ordini
            if (opMsg != null && opMsg.Operation == Operations.Insert)
            {
                logger.LogInformation("Richiesta pagamento per Ordine {IdOrdine}. Importo: {Importo}", opMsg.Dto.IdOrdine, opMsg.Dto.Importo);

                using var scope = serviceScopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

                await business.ElaboraPagamentoAsync(opMsg.Dto.IdOrdine, opMsg.Dto.IdCliente, opMsg.Dto.Importo, cancellationToken);
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