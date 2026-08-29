using Ordini.Repository.Model;
using Ordini.Shared;

namespace Ordini.Repository.Abstraction;

public interface IRepository
{
    // Metodi per la gestione della transazione e del salvataggio
    Task BeginTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Metodi per gli Ordini
    Task<Ordine> CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default);
    Task<Ordine?> UpdateStatoOrdineAsync(int idOrdine, string nuovoStato, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ordine>> GetOrdiniScadutiAsync(int minutiScadenza, CancellationToken cancellationToken = default);

    // Metodi per l'Outbox Pattern
    Task<IEnumerable<TransactionalOutbox>> GetAllTransactionalOutboxAsync(CancellationToken cancellationToken = default);
    Task<TransactionalOutbox?> GetTransactionalOutboxByKeyAsync(long id, CancellationToken cancellationToken = default);
    Task DeleteTransactionalOutboxAsync(long id, CancellationToken cancellationToken = default);
    Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default);
}