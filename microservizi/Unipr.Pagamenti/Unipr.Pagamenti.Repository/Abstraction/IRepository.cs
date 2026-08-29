using Pagamenti.Repository.Model;

namespace Pagamenti.Repository.Abstraction;

public interface IRepository
{
    // Transazione base
    Task BeginTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Metodi per i Pagamenti (La transazione Pivot)
    Task<ContoCliente?> GetContoByClienteAsync(int idCliente, CancellationToken cancellationToken = default);
    Task CreaOAggiornaContoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default);
    Task<bool> PrelevaFondiAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default);

    // Metodi Outbox
    Task<IEnumerable<TransactionalOutbox>> GetAllTransactionalOutboxAsync(CancellationToken cancellationToken = default);
    Task DeleteTransactionalOutboxAsync(long id, CancellationToken cancellationToken = default);
    Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default);
}