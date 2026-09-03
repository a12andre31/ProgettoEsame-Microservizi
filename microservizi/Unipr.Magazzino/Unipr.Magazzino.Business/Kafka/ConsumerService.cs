using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Magazzino.Business.Abstraction;
using Magazzino.Shared;
using Utility.Kafka.Abstractions.MessageHandlers;
using Utility.Kafka.Messages;
using Utility.Kafka.Constants;

namespace Magazzino.Business.Kafka;

//IL GESTORE
public class ConsumerHandler(
    ILogger<ConsumerHandler> logger,
    IServiceScopeFactory serviceScopeFactory) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken)
    {
        try
        {
            // Decifriamo la busta standard di Kafka
            var opMsg = JsonSerializer.Deserialize<OperationMessage<OrdineInArrivoDto>>(message);

            if (opMsg != null)
            {
                // Apriamo uno scope per usare IBusiness in modo sicuro
                using var scope = serviceScopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

                // Nuovo Ordine (Avanzamento SAGA)
                if (opMsg.Operation == Operations.Insert && opMsg.Dto.Stato == "Pending")
                {
                    logger.LogInformation("Nuovo ordine {Id}. Avvio prenotazione...", opMsg.Dto.Id);
                    // Lanciamo la logica SAGA
                    await business.ElaboraPrenotazioneAsync(opMsg.Dto.Id, opMsg.Dto.CodiceArticolo, opMsg.Dto.Quantita, cancellationToken);
                }
                // Ordine Annullato (Compensazione SAGA)
                else if (opMsg.Operation == Operations.Update &&
                        (opMsg.Dto.Stato == "PagamentoRifiutato" || opMsg.Dto.Stato == "Timeout_Magazzino" || opMsg.Dto.Stato == "Timeout_Pagamenti"))
                {
                    logger.LogWarning("Ordine {Id} annullato. Ripristino merce per {CodiceArticolo}.", opMsg.Dto.Id, opMsg.Dto.CodiceArticolo);
                    await business.AnnullaPrenotazioneAsync(opMsg.Dto.CodiceArticolo, opMsg.Dto.Quantita, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante la lettura del messaggio da Kafka.");
        }
    }
}

//LA FABBRICA
public class ConsumerHandlerFactory(
    ILogger<ConsumerHandler> logger,
    IServiceScopeFactory serviceScopeFactory) : IMessageHandlerFactory<string, string>
{
    public IMessageHandler<string, string> Create(string topic, IServiceProvider serviceProvider)
    {
        return new ConsumerHandler(logger, serviceScopeFactory);
    }
}