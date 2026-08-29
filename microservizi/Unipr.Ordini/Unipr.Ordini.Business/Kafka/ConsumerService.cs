using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Ordini.Business.Abstraction;
using Ordini.Shared;
using Utility.Kafka.Abstractions.MessageHandlers;
using Utility.Kafka.Messages;
using Utility.Kafka.Constants;

namespace Ordini.Business.Kafka;

public class ConsumerHandler(
    ILogger<ConsumerHandler> logger,
    IServiceScopeFactory serviceScopeFactory) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken)
    {
        try
        {
            var opMsg = JsonSerializer.Deserialize<OperationMessage<RispostaMagazzinoInArrivoDto>>(message);

            // Solo le nuove risposte inserite dal Magazzino
            if (opMsg != null && opMsg.Operation == Operations.Insert)
            {
                logger.LogInformation("Ricevuta risposta da Magazzino per Ordine {IdOrdine}: {Esito}", opMsg.Dto.IdOrdine, opMsg.Dto.Esito);

                using var scope = serviceScopeFactory.CreateScope();
                var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

                await business.GestisciRispostaMagazzinoAsync(opMsg.Dto.IdOrdine, opMsg.Dto.Esito, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore lettura messaggio Kafka in Ordini.");
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