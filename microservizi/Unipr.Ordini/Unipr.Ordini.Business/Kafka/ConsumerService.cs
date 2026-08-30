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
    IServiceScopeFactory serviceScopeFactory,
    string topic) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            if (topic == "Magazzino")
            {
                var opMsg = JsonSerializer.Deserialize<OperationMessage<RispostaMagazzinoInArrivoDto>>(message);
                if (opMsg != null && opMsg.Operation == Operations.Insert)
                {
                    await business.GestisciRispostaMagazzinoAsync(opMsg.Dto.IdOrdine, opMsg.Dto.Esito, cancellationToken);
                }
            }
            else if (topic == "Pagamenti")
            {
                var opMsg = JsonSerializer.Deserialize<OperationMessage<RispostaPagamentoInArrivoDto>>(message);
                if (opMsg != null && opMsg.Operation == Operations.Insert)
                {
                    await business.GestisciRispostaPagamentoAsync(opMsg.Dto.IdOrdine, opMsg.Dto.Esito, cancellationToken);
                }
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
        return new ConsumerHandler(logger, serviceScopeFactory, topic);
    }
}