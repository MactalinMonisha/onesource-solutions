using System.Globalization;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;

namespace OneSource.Api.Services;

public static class ProductCodeGenerator
{
    private const long AdvisoryLockKey = 736120011;

    public static Task AcquireLockAsync(AppDbContext db, CancellationToken cancellationToken = default) =>
        db.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({AdvisoryLockKey});",
            cancellationToken);

    public static async Task<string> GenerateNextAsync(
        AppDbContext db,
        IEnumerable<string>? reservedCodes = null,
        CancellationToken cancellationToken = default)
    {
        var existingCodes = await db.Products
            .AsNoTracking()
            .Where(product => product.ProductCode != null)
            .Select(product => product.ProductCode!)
            .ToListAsync(cancellationToken);

        var maxCode = BigInteger.Zero;
        foreach (var code in existingCodes.Concat(reservedCodes ?? []))
        {
            if (BigInteger.TryParse(code, NumberStyles.None, CultureInfo.InvariantCulture, out var numericCode)
                && numericCode > maxCode)
                maxCode = numericCode;
        }

        return (maxCode + BigInteger.One).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');
    }
}
