using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordini.Business.Abstraction;
using Ordini.Repository.Abstraction;
using Ordini.Repository.Model;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Services;

namespace Ordini.Business.Kafka;

public class ProducerServiceWithSubscription(
    ILogger<ProducerServiceWithSubscription> logger,
    IProducerClient<string, string> producerClient,
    IOptions<KafkaTopicsOutput> optionsTopics,
    IServiceProvider serviceProvider,
    IServiceScopeFactory serviceScopeFactory,
    IUniprOrdiniObservable observable)
    : AbstractProducerServiceWithSubscription(logger, serviceProvider)
{
    protected override IDisposable Subscribe(TaskCompletionSource tcs)
    {
        return observable.NuovoOrdine.Subscribe((change) => tcs.TrySetResult());
    }

    protected override IEnumerable<string> GetTopics()
    {
        return optionsTopics.Value.GetTopics();
    }

    protected override async Task OperationsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IRepository repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var transactionalOutboxList = (await repository.GetAllTransactionalOutboxAsync(cancellationToken)).OrderBy(x => x.Id);
        if (!transactionalOutboxList.Any()) return;

        foreach (TransactionalOutbox tran in transactionalOutboxList)
        {
            try
            {
                //capisce dove spedire
                string topic = tran.Tabella switch
                {
                    nameof(Ordine) => optionsTopics.Value.Ordini,
                    _ => throw new ArgumentOutOfRangeException($"La tabella {tran.Tabella} non è gestita")
                };

                //spedisce a kafka
                await producerClient.ProduceAsync(topic, tran.Id.ToString(), tran.Messaggio, cancellationToken);

                //cancello il messaggio dal database
                await repository.DeleteTransactionalOutboxAsync(tran.Id, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore invio messaggio Outbox ID: {id}", tran.Id);
            }
        }
    }
}