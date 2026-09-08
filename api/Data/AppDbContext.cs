using Microsoft.EntityFrameworkCore;
using OneSource.Api.Models;

namespace OneSource.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<TrustedCompany> TrustedCompanies => Set<TrustedCompany>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasKey(c => c.CateId);
            e.Property(c => c.CatName).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.ProductId);
            e.Property(p => p.ProductName).IsRequired().HasMaxLength(200);
            e.Property(p => p.ProductDescription).HasMaxLength(1000);

            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasKey(pi => pi.ProductImageId);
            e.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(500);

            e.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.UserId);
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.Property(u => u.MailId).IsRequired().HasMaxLength(255);
            e.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Contact>(e =>
        {
            e.HasKey(c => c.ContactId);
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.WhatsApp).HasMaxLength(20);
            e.Property(c => c.Email).HasMaxLength(255);
            e.Property(c => c.Location).HasMaxLength(500);
            e.Property(c => c.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<Offer>(e =>
        {
            e.HasKey(o => o.OfferId);
            e.Property(o => o.OfferName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<TrustedCompany>(e =>
        {
            e.HasKey(c => c.TrustedCompanyId);
            e.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = now;
                    entry.Entity.UpdatedDate = now;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(IAuditableEntity.CreatedDate)).IsModified = false;
                    entry.Entity.UpdatedDate = now;
                    break;
            }
        }
    }
}
