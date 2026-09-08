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
}
