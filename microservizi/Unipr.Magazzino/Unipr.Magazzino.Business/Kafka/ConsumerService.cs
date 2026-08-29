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

            // Solo i nuovi ordini (Insert) nello stato "Pending"
            if (opMsg != null && opMsg.Operation == Operations.Insert && opMsg.Dto.Stato == "Pending")
            {
                logger.LogInformation("Ricevuto nuovo ordine {IdOrdine}. Avvio elaborazione...", opMsg.Dto.Id);

                // Apriamo uno scope per usare IBusiness in modo sicuro
                using var scope = serviceScopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

                // Lanciamo la nostra logica SAGA
                await business.ElaboraPrenotazioneAsync(opMsg.Dto.Id, opMsg.Dto.CodiceArticolo, opMsg.Dto.Quantita, cancellationToken);
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