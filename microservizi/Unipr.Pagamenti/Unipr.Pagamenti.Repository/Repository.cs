using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pagamenti.Repository.Abstraction;
using Pagamenti.Repository.Model;

namespace Pagamenti.Repository;

public class Repository(PagamentiDbContext dbContext) : IRepository
{
    // GESTIONE TRANSAZIONE 
    public async Task BeginTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction != null)
        {
            await action(cancellationToken);
        }
        else
        {
            await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    // METODI CONTO CLIENTE (Transazione Pivot)
    public async Task<ContoCliente?> GetContoByClienteAsync(int idCliente, CancellationToken cancellationToken = default)
    {
        return await dbContext.ContiClienti.FirstOrDefaultAsync(x => x.IdCliente == idCliente, cancellationToken);
    }

    public async Task CreaOAggiornaContoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default)
    {
        var conto = await GetContoByClienteAsync(idCliente, cancellationToken);

        if (conto == null)
        {
            // Se non esiste, lo creiamo da zero
            await dbContext.ContiClienti.AddAsync(new ContoCliente { IdCliente = idCliente, Saldo = importo }, cancellationToken);
        }
        else
        {
            // Se esiste già, sommiamo i soldi al saldo attuale
            conto.Saldo += importo;
        }
    }

    public async Task<bool> PrelevaFondiAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default)
    {
        var conto = await GetContoByClienteAsync(idCliente, cancellationToken);

        // Se il conto esiste e ci sono abbastanza soldi, scaliamo il saldo (PIVOT = TRUE)
        if (conto != null && conto.Saldo >= importo)
        {
            conto.Saldo -= importo;
            return true;
        }

        // Altrimenti la transazione fallisce (PIVOT = FALSE)
        return false;
    }

    #region TransactionalOutbox

    public async Task<IEnumerable<TransactionalOutbox>> GetAllTransactionalOutboxAsync(CancellationToken cancellationToken = default)
        => await dbContext.TransactionalOutboxList.ToListAsync(cancellationToken);

    public async Task<TransactionalOutbox?> GetTransactionalOutboxByKeyAsync(long id, CancellationToken cancellationToken = default)
    {
        return await dbContext.TransactionalOutboxList.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task DeleteTransactionalOutboxAsync(long id, CancellationToken cancellationToken = default)
    {
        dbContext.TransactionalOutboxList.Remove(
            (await GetTransactionalOutboxByKeyAsync(id, cancellationToken)) ??
            throw new ArgumentException($"Outbox con id {id} non trovato", nameof(id)));
    }

    public async Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default)
    {
        await dbContext.TransactionalOutboxList.AddAsync(transactionalOutbox, cancellationToken);
    }

    #endregion
}