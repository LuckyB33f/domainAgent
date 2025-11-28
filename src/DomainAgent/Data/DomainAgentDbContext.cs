using DomainAgent.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainAgent.Data;

/// <summary>
/// Database context for Domain Agent application.
/// </summary>
public class DomainAgentDbContext : DbContext
{
    public DomainAgentDbContext(DbContextOptions<DomainAgentDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Drop list entries from various sources.
    /// </summary>
    public DbSet<DropListEntry> DropListEntries { get; set; }

    /// <summary>
    /// Domain purchase records.
    /// </summary>
    public DbSet<Purchase> Purchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DropListEntry
        modelBuilder.Entity<DropListEntry>(entity =>
        {
            entity.ToTable("drop_list_entries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DomainName)
                .HasColumnName("domain_name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.DropDate)
                .HasColumnName("drop_date");

            entity.Property(e => e.Tld)
                .HasColumnName("tld")
                .HasMaxLength(50);

            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            // Create index on domain name and drop date for efficient lookups
            entity.HasIndex(e => new { e.DomainName, e.DropDate })
                .HasDatabaseName("ix_drop_list_entries_domain_drop_date");

            entity.HasIndex(e => e.DropDate)
                .HasDatabaseName("ix_drop_list_entries_drop_date");
        });

        // Configure Purchase
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("purchases");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DomainName)
                .HasColumnName("domain_name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Tld)
                .HasColumnName("tld")
                .HasMaxLength(50);

            entity.Property(e => e.OrderId)
                .HasColumnName("order_id")
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(1000);

            entity.Property(e => e.PurchaseDate)
                .HasColumnName("purchase_date");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            // Create indexes for common queries
            entity.HasIndex(e => e.DomainName)
                .HasDatabaseName("ix_purchases_domain_name");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_purchases_status");

            entity.HasIndex(e => e.PurchaseDate)
                .HasDatabaseName("ix_purchases_purchase_date");
        });
    }
}
