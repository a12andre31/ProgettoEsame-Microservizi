using Microsoft.EntityFrameworkCore;
using Pagamenti.Repository.Model;

namespace Pagamenti.Repository;

public class PagamentiDbContext(DbContextOptions<PagamentiDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContoCliente>().HasKey(x => x.Id);
        modelBuilder.Entity<ContoCliente>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<ContoCliente>().Property(x => x.Saldo).HasPrecision(18, 2);

        modelBuilder.Entity<TransactionalOutbox>().HasKey(e => new { e.Id });
        modelBuilder.Entity<TransactionalOutbox>().Property(e => e.Id).ValueGeneratedOnAdd();
    }

    public DbSet<ContoCliente> ContiClienti { get; set; }
    public DbSet<TransactionalOutbox> TransactionalOutboxList { get; set; }
}