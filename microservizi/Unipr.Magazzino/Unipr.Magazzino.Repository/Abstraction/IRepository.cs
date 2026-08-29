using Magazzino.Repository.Model;
using Magazzino.Shared;

namespace Magazzino.Repository.Abstraction;

public interface IRepository
{
    Task BeginTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Articolo?> GetArticoloByCodiceAsync(string codiceArticolo, CancellationToken cancellationToken = default);
    Task CreateOrUpdateArticoloAsync(ArticoloInsertDto dto, CancellationToken cancellationToken = default);

    // Metodi Outbox uguali a Ordini
    Task<IEnumerable<TransactionalOutbox>> GetAllTransactionalOutboxAsync(CancellationToken cancellationToken = default);
    Task DeleteTransactionalOutboxAsync(long id, CancellationToken cancellationToken = default);
    Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default);
}