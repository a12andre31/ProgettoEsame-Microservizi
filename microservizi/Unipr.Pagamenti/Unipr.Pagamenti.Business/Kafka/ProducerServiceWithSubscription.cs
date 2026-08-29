using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pagamenti.Business.Abstraction;
using Pagamenti.Repository.Abstraction;
using Pagamenti.Repository.Model;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Services;

namespace Pagamenti.Business.Kafka;

public class ProducerServiceWithSubscription(
    ILogger<ProducerServiceWithSubscription> logger,
    IProducerClient<string, string> producerClient,
    IOptions<KafkaTopicsOutput> optionsTopics,
    IServiceProvider serviceProvider,
    IServiceScopeFactory serviceScopeFactory,
    IUniprPagamentiObservable observable)
    : AbstractProducerServiceWithSubscription(logger, serviceProvider)
{
    protected override IDisposable Subscribe(TaskCompletionSource tcs)
    {
        return observable.NuovoPagamento.Subscribe((change) => tcs.TrySetResult());
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
                string topic = tran.Tabella switch
                {
                    "Pagamento" => optionsTopics.Value.Pagamenti,
                    _ => throw new ArgumentOutOfRangeException($"La tabella {tran.Tabella} non è gestita")
                };

                await producerClient.ProduceAsync(topic, tran.Id.ToString(), tran.Messaggio, cancellationToken);
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