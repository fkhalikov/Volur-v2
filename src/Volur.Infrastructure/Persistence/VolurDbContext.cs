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
    public DbSet<ExchangeEntity> Exchanges { get; set; }
    public DbSet<SymbolEntity> Symbols { get; set; }
    public DbSet<StockQuoteEntity> StockQuotes { get; set; }
    public DbSet<StockFundamentalsEntity> StockFundamentals { get; set; }
    public DbSet<NoDataAvailableEntity> NoDataAvailable { get; set; }

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
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => new { e.Ticker, e.ExchangeCode });
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure StockKeyValue entity
        modelBuilder.Entity<StockKeyValue>(entity =>
        {
            entity.ToTable("StockKeyValues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.HasIndex(e => new { e.Ticker, e.ExchangeCode, e.Key });
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure ExchangeEntity
        modelBuilder.Entity<ExchangeEntity>(entity =>
        {
            entity.ToTable("Exchanges");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OperatingMic).HasMaxLength(50);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure SymbolEntity
        modelBuilder.Entity<SymbolEntity>(entity =>
        {
            entity.ToTable("Symbols");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ParentExchange).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FullSymbol).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Isin).HasMaxLength(20);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Sector).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(200);
            
            entity.HasIndex(e => e.FullSymbol).IsUnique();
            entity.HasIndex(e => e.ExchangeCode);
            entity.HasIndex(e => e.ParentExchange);
            entity.HasIndex(e => e.Ticker);
            entity.HasIndex(e => e.Type);
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure StockQuoteEntity
        modelBuilder.Entity<StockQuoteEntity>(entity =>
        {
            entity.ToTable("StockQuotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Ticker).IsUnique();
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            entity.Property(e => e.LastUpdated).IsRequired();
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure StockFundamentalsEntity
        modelBuilder.Entity<StockFundamentalsEntity>(entity =>
        {
            entity.ToTable("StockFundamentals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Ticker).IsUnique();
            entity.Property(e => e.CompanyName).HasMaxLength(500);
            entity.Property(e => e.Sector).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(200);
            entity.Property(e => e.Description);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10);
            entity.Property(e => e.CurrencySymbol).HasMaxLength(10);
            entity.Property(e => e.CurrencyName).HasMaxLength(100);
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            entity.Property(e => e.LastUpdated).IsRequired();
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Configure NoDataAvailableEntity
        modelBuilder.Entity<NoDataAvailableEntity>(entity =>
        {
            entity.ToTable("NoDataAvailable");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.LastErrorMessage).HasMaxLength(2000);
            entity.HasIndex(e => new { e.Ticker, e.ExchangeCode }).IsUnique();
            entity.HasIndex(e => e.ExchangeCode);
            
            // Timestamp properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            entity.Property(e => e.FirstFailedAt).IsRequired();
            entity.Property(e => e.LastAttemptedAt).IsRequired();
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
    }
}
