using Microsoft.EntityFrameworkCore;
using SistemaFornitori.Models;

namespace SistemaFornitori.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ArticoloFornitore> ArticoliFornitori => Set<ArticoloFornitore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticoloFornitore>(entity =>
        {
            entity.ToTable("ArticoliFornitori");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.EAN).IsUnique();
            entity.HasIndex(e => e.Fornitore);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.Colore);
            entity.HasIndex(e => e.Taglia);
            entity.HasIndex(e => e.DataImport);

            entity.Property(e => e.EAN).HasMaxLength(13).IsRequired();
            entity.Property(e => e.Articolo).HasMaxLength(200);
            entity.Property(e => e.Fornitore).HasMaxLength(100);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Taglia).HasMaxLength(20);
            entity.Property(e => e.Colore).HasMaxLength(50);
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.Property(e => e.Prezzo).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DataImport).HasDefaultValueSql("GETDATE()");
        });
    }
}