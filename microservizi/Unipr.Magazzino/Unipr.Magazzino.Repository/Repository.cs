using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Magazzino.Repository.Abstraction;
using Magazzino.Repository.Model;
using Magazzino.Shared;

namespace Magazzino.Repository;

public class Repository(MagazzinoDbContext dbContext) : IRepository
{
    //GESTIONE TRANSAZIONE
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

    //METODI MAGAZZINO
    public async Task<Articolo?> GetArticoloByCodiceAsync(string codiceArticolo, CancellationToken cancellationToken = default)
    {
        return await dbContext.Articoli.FirstOrDefaultAsync(x => x.CodiceArticolo == codiceArticolo, cancellationToken);
    }

    public async Task CreateOrUpdateArticoloAsync(ArticoloInsertDto dto, CancellationToken cancellationToken = default)
    {
        var articolo = await GetArticoloByCodiceAsync(dto.CodiceArticolo, cancellationToken);
        if (articolo == null)
        {
            await dbContext.Articoli.AddAsync(new Articolo { CodiceArticolo = dto.CodiceArticolo, QuantitaDisponibile = dto.Quantita }, cancellationToken);
        }
        else
        {
            articolo.QuantitaDisponibile += dto.Quantita;
        }
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
            throw new ArgumentException($"TransactionalOutbox con id {id} non trovato", nameof(id)));
    }

    public async Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default)
    {
        await dbContext.TransactionalOutboxList.AddAsync(transactionalOutbox, cancellationToken);
    }

    #endregion
}