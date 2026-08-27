using AutoMapper;
using Microsoft.Extensions.Logging;
using Ordini.Business.Abstraction;
using Ordini.Business.Factory;
using Ordini.Repository.Abstraction;
using Ordini.Shared;

namespace Ordini.Business;

public class Business(IRepository repository, ILogger<Business> logger, IMapper map, IUniprOrdiniObserver observer) : IBusiness
{
    public async Task CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default)
    {
        // Avviamo la transazione sicura
        await repository.BeginTransactionAsync(async (CancellationToken cancellation) =>
        {
            // 1. Salviamo l'ordine sul DB
            var ordine = await repository.CreateOrdineAsync(ordineInsertDto, cancellation);
            await repository.SaveChangesAsync(cancellation); // Salvataggio intermedio per fargli generare l'Id

            // 2. Travasiamo il model sul DTO di lettura (che ora contiene l'Id autogenerato e lo stato "Pending")
            var newOrdineRecord = map.Map<OrdineReadDto>(ordine);

            // 3. Creiamo il messaggio per Kafka e lo inseriamo nella tabella TransactionalOutboxList
            await repository.InsertTransactionalOutboxAsync(TransactionalOutboxFactory.CreateInsert(newOrdineRecord), cancellation);
            await repository.SaveChangesAsync(cancellation); // Conferma definitiva

        }, cancellationToken);

        // Notifichiamo in tempo reale al servizio in background che c'è un nuovo messaggio da spedire
        observer.NuovoOrdine.OnNext(1);
    }
}