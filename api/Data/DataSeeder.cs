using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.Models;

namespace OneSource.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAdminUserAsync(db);
        await SeedContactAsync(db);
        await SeedAboutContentAsync(db);
        await SeedHomeContentAsync(db);
        await SeedCarouselSlidesAsync(db);
        await SeedTrustedCompaniesAsync(db);
        await SeedOffersAsync(db);

        if (await db.ProductCategories.AnyAsync()) return;

        var categories = new Dictionary<string, ProductCategory>
        {
            ["tims"] = new() { CatName = "tims", Description = "TIMS Devices" },
            ["etims"] = new() { CatName = "etims", Description = "eTIMS Software" },
            ["pos"] = new() { CatName = "pos", Description = "Point of Sale Equipment" },
            ["it"] = new() { CatName = "it", Description = "IT Infrastructure" },
            ["security"] = new() { CatName = "security", Description = "Security Systems" },
        };

        await db.ProductCategories.AddRangeAsync(categories.Values);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            // TIMS
            new() { ProductName = "K10 Electronic Tax Register", CatId = categories["tims"].CateId },
            new() { ProductName = "Ace Control Unit", CatId = categories["tims"].CateId },

            // eTIMS
            new() { ProductName = "Hospitality & Restaurant Systems", CatId = categories["etims"].CateId },
            new() { ProductName = "Payroll & HR Systems", CatId = categories["etims"].CateId },

            // POS
            new() { ProductName = "Phoenix Cash Drawer", CatId = categories["pos"].CateId },
            new() { ProductName = "Phoenix BC25 Table Scanner", CatId = categories["pos"].CateId },
            new() { ProductName = "Phoenix BC108 Wireless Scanner", CatId = categories["pos"].CateId },
            new() { ProductName = "Phoenix Bluetooth Thermal Printer", CatId = categories["pos"].CateId },
            new() { ProductName = "Thermal Paper Rolls", CatId = categories["pos"].CateId },

            // IT
            new() { ProductName = "Structured Cabling & Networking", CatId = categories["it"].CateId },
            new() { ProductName = "Server & IT Infrastructure Setup", CatId = categories["it"].CateId },
            new() { ProductName = "Annual Maintenance Contracts", CatId = categories["it"].CateId },

            // Security
            new() { ProductName = "CCTV Systems with Remote Monitoring", CatId = categories["security"].CateId },
            new() { ProductName = "PTZ & Dome Cameras", CatId = categories["security"].CateId },
            new() { ProductName = "Scalable DVR Systems", CatId = categories["security"].CateId },
        };

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCarouselSlidesAsync(AppDbContext db)
    {
        if (await db.CarouselSlides.AnyAsync()) return;

        db.CarouselSlides.AddRange(
            new CarouselSlide
            {
                ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=2000&q=85",
                AltText = "Rows of server racks in a modern data center",
                Label = "Reliable infrastructure",
                Description = "Dependable technology and infrastructure to keep your business connected.",
                Title = "Technology built for business",
                SortOrder = 0
            },
            new CarouselSlide
            {
                ImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=2000&q=85",
                AltText = "Close-up of modern computer components and circuit board",
                Label = "Hardware and systems",
                Description = "Business-ready hardware and systems selected to support your day-to-day work.",
                Title = "The right tools for every workday",
                SortOrder = 1
            },
            new CarouselSlide
            {
                ImageUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=2000&q=85",
                AltText = "Bright, connected modern office workspace",
                Label = "Support that stays with you",
                Description = "Practical guidance and ongoing support from setup through everyday use.",
                Title = "A practical partner for your next step",
                SortOrder = 2
            });

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Username = "admin",
            MailId = "admin@onesourcesolutions.co.ke",
            Role = "Admin",
        };

        var hasher = new PasswordHasher<User>();
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedContactAsync(AppDbContext db)
    {
        if (await db.Contacts.AnyAsync()) return;

        db.Contacts.Add(new Contact
        {
            Phone = "+254207903190",
            WhatsApp = "+254735610610",
            Email = "info@onesourcesolutionske.com",
            Location = "Office No 7E, 7th Floor, TRV Centre Building, 3rd Avenue Parklands, Nairobi",
            Website = "https://www.onesourcesolutionske.com",
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedAboutContentAsync(AppDbContext db)
    {
        var about = await db.AboutContents.FirstOrDefaultAsync(a => a.AboutContentId == 1);
        if (about is null)
        {
            about = new AboutContent { AboutContentId = 1 };
            db.AboutContents.Add(about);
        }

        if (string.IsNullOrWhiteSpace(about.Heading))
            about.Heading = "Technology that helps businesses move forward";
        if (string.IsNullOrWhiteSpace(about.Description))
        {
            about.Description = "OneSource Solutions Limited is a Nairobi-based ICT partner helping businesses work smarter through dependable technology. We bring together business hardware, tax-compliant systems, software, networking, and security solutions, with practical guidance from selection through implementation and ongoing support.\n\nOur team works with organizations across retail, hospitality, healthcare, and other sectors to match the right technology to the way each business operates.";
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedHomeContentAsync(AppDbContext db)
    {
        if (await db.HomeContents.AnyAsync()) return;

        db.HomeContents.Add(new HomeContent
        {
            HomeContentId = 1,
            Heading = "Empowering You with the Latest in IT & Technology",
            Description = "OneSource Solutions Limited is an ICT company providing customized hardware and software solutions to enable vital business processes across varied sectors. As KRA authorized suppliers of Electronic Fiscal Devices and providers of hospitality, payroll, networking, and CCTV solutions, we're your complete technology partner in Nairobi."
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedTrustedCompaniesAsync(AppDbContext db)
    {
        if (await db.TrustedCompanies.AnyAsync()) return;

        var names = new[]
        {
            "Total Energies Limited",
            "Kenya Ports Authority",
            "Kenafric Industries Limited",
            "Tropical Heat Limited",
            "Hotpoint Limited",
            "Healthy U Ltd",
            "Roche Kenya Ltd",
            "Ramco Group of Companies",
            "Simbisa Brands Kenya Limited",
            "Good Life Pharmacy",
            "Airtel Kenya Limited",
            "Auto Xpress Limited",
            "Mastermind Tobacco Limited",
            "Black Tulip Group of Companies",
            "Kingsway Tyres Limited",
            "CFAO Kenya Ltd (Toyota)",
            "PKF Kenya Limited",
            "Kenya Revenue Authority",
            "Dalbit Petroleum Limited",
        };

        db.TrustedCompanies.AddRange(names.Select(n => new TrustedCompany { CompanyName = n }));
        await db.SaveChangesAsync();
    }

    private static async Task SeedOffersAsync(AppDbContext db)
    {
        if (await db.Offers.AnyAsync()) return;

        db.Offers.Add(new Offer
        {
            OfferName = "10% Offer",
            Description = "Purchase with product code HRKDNS908",
        });

        await db.SaveChangesAsync();
    }
}
