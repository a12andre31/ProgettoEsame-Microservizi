using Microsoft.EntityFrameworkCore;
using Ordini.Repository.Model;

namespace Ordini.Repository;

public class OrdiniDbContext(DbContextOptions<OrdiniDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    //Fluent api
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurazione Tabella Ordini
        modelBuilder.Entity<Ordine>().HasKey(x => x.Id);
        modelBuilder.Entity<Ordine>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Ordine>().Property(x => x.PrezzoTotale).HasPrecision(18, 2);

        // Configurazione Tabella per l'Outbox Pattern di Kafka
        modelBuilder.Entity<TransactionalOutbox>().HasKey(e => new { e.Id });
        modelBuilder.Entity<TransactionalOutbox>().Property(e => e.Id).ValueGeneratedOnAdd();
    }

    public DbSet<Ordine> Ordini { get; set; }
    public DbSet<TransactionalOutbox> TransactionalOutboxList { get; set; }
}