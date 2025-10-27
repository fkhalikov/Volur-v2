using Microsoft.EntityFrameworkCore;
using Volur.Domain.Entities;

namespace Volur.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for SQL Server database.
/// </summary>
public sealed class VolurDbContext : DbContext
{
    public DbSet<StockNote> StockNotes { get; set; }
    public DbSet<StockKeyValue> StockKeyValues { get; set; }

    public VolurDbContext(DbContextOptions<VolurDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure StockNote entity
        modelBuilder.Entity<StockNote>(entity =>
        {
            entity.ToTable("StockNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => new { e.Ticker, e.ExchangeCode });
        });

        // Configure StockKeyValue entity
        modelBuilder.Entity<StockKeyValue>(entity =>
        {
            entity.ToTable("StockKeyValues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.HasIndex(e => new { e.Ticker, e.ExchangeCode, e.Key });
        });
    }
}
