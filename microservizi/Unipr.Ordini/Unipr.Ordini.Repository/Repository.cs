using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ordini.Repository.Abstraction;
using Ordini.Repository.Model;
using Ordini.Shared;

namespace Ordini.Repository;

public class Repository(OrdiniDbContext ordiniDbContext) : IRepository
{
    //Gestione della Transazione Sicura
    public async Task BeginTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (ordiniDbContext.Database.CurrentTransaction != null)
        {
            await action(cancellationToken);
        }
        else
        {
            await using IDbContextTransaction transaction = await ordiniDbContext.Database.BeginTransactionAsync(cancellationToken);
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
        return await ordiniDbContext.SaveChangesAsync(cancellationToken);
    }

    //Creazione dell'Ordine
    public async Task<Ordine> CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default)
    {
        Ordine ordine = new Ordine()
        {
            CodiceArticolo = ordineInsertDto.CodiceArticolo,
            Quantita = ordineInsertDto.Quantita,
            PrezzoTotale = ordineInsertDto.PrezzoTotale,
            IdCliente = ordineInsertDto.IdCliente,
            Stato = "Pending", // Stato iniziale della SAGA
            DataCreazione = DateTime.UtcNow
        };

        await ordiniDbContext.Ordini.AddAsync(ordine, cancellationToken);
        return ordine;
    }

    //Aggiornamento dello Stato dell'Ordine
    public async Task<Ordine?> UpdateStatoOrdineAsync(int idOrdine, string nuovoStato, CancellationToken cancellationToken = default)
    {
        var ordine = await ordiniDbContext.Ordini.FirstOrDefaultAsync(x => x.Id == idOrdine, cancellationToken);
        if (ordine != null)
        {
            ordine.Stato = nuovoStato;
        }
        return ordine;
    }

    #region TransactionalOutbox

    public async Task<IEnumerable<TransactionalOutbox>> GetAllTransactionalOutboxAsync(CancellationToken cancellationToken = default)
        => await ordiniDbContext.TransactionalOutboxList.ToListAsync(cancellationToken);

    public async Task<TransactionalOutbox?> GetTransactionalOutboxByKeyAsync(long id, CancellationToken cancellationToken = default)
    {
        return await ordiniDbContext.TransactionalOutboxList.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task DeleteTransactionalOutboxAsync(long id, CancellationToken cancellationToken = default)
    {
        ordiniDbContext.TransactionalOutboxList.Remove(
            (await GetTransactionalOutboxByKeyAsync(id, cancellationToken)) ??
            throw new ArgumentException($"TransactionalOutbox con id {id} non trovato", nameof(id)));
    }

    public async Task InsertTransactionalOutboxAsync(TransactionalOutbox transactionalOutbox, CancellationToken cancellationToken = default)
    {
        await ordiniDbContext.TransactionalOutboxList.AddAsync(transactionalOutbox, cancellationToken);
    }

    #endregion
}