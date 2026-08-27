using Magazzino.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Magazzino.Repository;

public class MagazzinoDbContext(DbContextOptions<MagazzinoDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Articolo>().HasKey(x => x.Id);
        modelBuilder.Entity<Articolo>().Property(e => e.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<TransactionalOutbox>().HasKey(e => new { e.Id });
        modelBuilder.Entity<TransactionalOutbox>().Property(e => e.Id).ValueGeneratedOnAdd();
    }

    public DbSet<Articolo> Articoli { get; set; }
    public DbSet<TransactionalOutbox> TransactionalOutboxList { get; set; }
}