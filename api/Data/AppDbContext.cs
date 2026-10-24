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
    public DbSet<ContactDetail> ContactDetails => Set<ContactDetail>();
    public DbSet<CarouselSlide> CarouselSlides => Set<CarouselSlide>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<TrustedCompany> TrustedCompanies => Set<TrustedCompany>();
    public DbSet<AboutContent> AboutContents => Set<AboutContent>();
    public DbSet<AboutImage> AboutImages => Set<AboutImage>();
    public DbSet<HomeContent> HomeContents => Set<HomeContent>();

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
            e.Property(p => p.ProductCode).HasMaxLength(100);
            e.HasIndex(p => p.ProductCode).IsUnique();

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
            e.Property(c => c.EnquiryWhatsAppNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<ContactDetail>(e =>
        {
            e.HasKey(d => d.ContactDetailId);
            e.Property(d => d.Label).IsRequired().HasMaxLength(100);
            e.Property(d => d.Value).IsRequired().HasMaxLength(500);
            e.Property(d => d.Type).IsRequired().HasMaxLength(20);

            e.HasOne(d => d.Contact)
                .WithMany(c => c.Details)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarouselSlide>(e =>
        {
            e.HasKey(slide => slide.CarouselSlideId);
            e.Property(slide => slide.ImageUrl).IsRequired().HasMaxLength(1000);
            e.Property(slide => slide.AltText).IsRequired().HasMaxLength(250);
            e.Property(slide => slide.Label).IsRequired().HasMaxLength(120);
            e.Property(slide => slide.Description).IsRequired().HasMaxLength(500);
            e.Property(slide => slide.Title).IsRequired().HasMaxLength(200);
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
            e.Property(c => c.LogoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<AboutContent>(e =>
        {
            e.HasKey(a => a.AboutContentId);
            e.Property(a => a.Heading).IsRequired().HasMaxLength(250);
            e.Property(a => a.Description).IsRequired().HasMaxLength(5000);

            e.HasMany(a => a.Images)
                .WithOne(i => i.AboutContent)
                .HasForeignKey(i => i.AboutContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AboutImage>(e =>
        {
            e.HasKey(i => i.AboutImageId);
            e.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<HomeContent>(e =>
        {
            e.HasKey(h => h.HomeContentId);
            e.Property(h => h.Heading).IsRequired().HasMaxLength(250);
            e.Property(h => h.Description).IsRequired().HasMaxLength(5000);
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
