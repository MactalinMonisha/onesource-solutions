using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;
using OneSource.Api.Services;
using System.Xml;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/Products")]
[Authorize(Roles = "Admin")]
public class ProductSpreadsheetController(AppDbContext db, ILogger<ProductSpreadsheetController> logger) : ControllerBase
{
    private static readonly string[] Headers =
    [
        "Product Cate(*)",
        "Product Cate Desc",
        "Product Code",
        "Product(*)",
        "Product Desc"
    ];

    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        WriteHeaders(sheet);
        SetProductColumnWidths(sheet);
        sheet.SheetView.FreezeRows(1);

        var instructions = workbook.Worksheets.Add("Instructions");
        instructions.Cell(1, 1).Value = "Product migration template";
        instructions.Cell(2, 1).Value = "Fill in one product per row on the Products sheet.";
        instructions.Cell(3, 1).Value = "Product Cate(*) and Product(*) are required.";
        instructions.Cell(4, 1).Value = "Product Code is optional. Blank codes are generated automatically; matching codes update existing products.";
        instructions.Cell(5, 1).Value = "A category is created if it does not exist. Matching category names are reused.";
        instructions.Column(1).Width = 105;
        instructions.Range("A1:A5").Style.Alignment.WrapText = true;
        instructions.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "product-import-template.xlsx");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var products = await db.Products
            .Where(product => product.IsActive && product.Category!.IsActive)
            .OrderBy(product => product.Category!.CatName)
            .ThenBy(product => product.ProductName)
            .Select(product => new
            {
                CategoryName = product.Category!.CatName,
                CategoryDescription = product.Category.Description,
                product.ProductCode,
                product.ProductName,
                product.ProductDescription
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        WriteHeaders(sheet);

        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            var row = index + 2;
            sheet.Cell(row, 1).Value = product.CategoryName;
            sheet.Cell(row, 2).Value = product.CategoryDescription;
            sheet.Cell(row, 3).Value = product.ProductCode;
            sheet.Cell(row, 4).Value = product.ProductName;
            sheet.Cell(row, 5).Value = product.ProductDescription;
        }

        SetProductColumnWidths(sheet);
        sheet.RangeUsed()!.Style.Alignment.WrapText = true;
        sheet.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products-export.xlsx");
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<ProductImportResultDto>> Import([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("Select a non-empty Excel workbook.");
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx workbooks are supported.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        XLWorkbook parsedWorkbook;
        try
        {
            parsedWorkbook = new XLWorkbook(stream);
        }
        catch (Exception exception) when (exception is IOException or ArgumentException or InvalidDataException or XmlException or OpenXmlPackageException)
        {
            logger.LogWarning(exception, "An admin uploaded an invalid product import workbook.");
            return BadRequest("The uploaded file is not a valid Excel workbook.");
        }

        using var workbook = parsedWorkbook;
        var sheet = workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == "Products") ?? workbook.Worksheet(1);
        if (sheet.LastRowUsed() is null) return BadRequest("The workbook is empty.");

        var headerRow = sheet.Row(1);
        var headers = Enumerable.Range(1, Headers.Length)
            .Select(column => headerRow.Cell(column).GetString().Trim())
            .ToArray();
        if (!headers.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
            return BadRequest($"Use the provided template with these columns: {string.Join(", ", Headers)}.");

        var lastRow = sheet.LastRowUsed()!.RowNumber();
        if (lastRow > 5001) return BadRequest("A workbook can contain at most 5,000 product rows.");

        var rows = new List<ImportRow>();
        var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = sheet.Row(rowNumber);
            if (row.Cells(1, Headers.Length).All(cell => cell.IsEmpty())) continue;

            var categoryName = row.Cell(1).GetString().Trim();
            var categoryDescription = row.Cell(2).GetString().Trim();
            var productCode = row.Cell(3).GetString().Trim();
            var productName = row.Cell(4).GetString().Trim();
            var productDescription = row.Cell(5).GetString().Trim();

            if (string.IsNullOrWhiteSpace(categoryName) || categoryName.Length > 150)
                return BadRequest($"Row {rowNumber}: Product Cate(*) is required and must be 150 characters or fewer.");
            if (categoryDescription.Length > 1000)
                return BadRequest($"Row {rowNumber}: Product Cate Desc must be 1,000 characters or fewer.");
            if (productCode.Length > 100)
                return BadRequest($"Row {rowNumber}: Product Code must be 100 characters or fewer.");
            if (string.IsNullOrWhiteSpace(productName) || productName.Length > 200)
                return BadRequest($"Row {rowNumber}: Product(*) is required and must be 200 characters or fewer.");
            if (productDescription.Length > 1000)
                return BadRequest($"Row {rowNumber}: Product Desc must be 1,000 characters or fewer.");
            if (productCode.Length > 0 && !codesInFile.Add(productCode))
                return BadRequest($"Row {rowNumber}: Product Code '{productCode}' appears more than once in this workbook.");

            rows.Add(new ImportRow(categoryName, categoryDescription, productCode, productName, productDescription));
        }

        if (rows.Count == 0) return BadRequest("The workbook has no product rows to import.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await ProductCodeGenerator.AcquireLockAsync(db);
        var categories = await db.ProductCategories.ToListAsync();
        var categoriesByName = categories
            .GroupBy(category => category.CatName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(category => category.IsActive).First(), StringComparer.OrdinalIgnoreCase);
        var categoriesCreated = 0;

        foreach (var row in rows)
        {
            if (!categoriesByName.TryGetValue(row.CategoryName, out var category))
            {
                category = new ProductCategory { CatName = row.CategoryName, Description = row.CategoryDescription };
                db.ProductCategories.Add(category);
                categoriesByName.Add(row.CategoryName, category);
                categoriesCreated++;
            }
            else
            {
                category.IsActive = true;
                if (!string.IsNullOrWhiteSpace(row.CategoryDescription))
                    category.Description = row.CategoryDescription;
            }
        }

        await db.SaveChangesAsync();

        var products = await db.Products.ToListAsync();
        var productsByCode = products
            .Where(product => !string.IsNullOrWhiteSpace(product.ProductCode))
            .GroupBy(product => product.ProductCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var created = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var category = categoriesByName[row.CategoryName];
            var productCode = row.ProductCode;
            if (productCode.Length > 0 && productsByCode.TryGetValue(productCode, out var existingProduct))
            {
                existingProduct.CatId = category.CateId;
                existingProduct.ProductName = row.ProductName;
                existingProduct.ProductDescription = row.ProductDescription;
                existingProduct.IsActive = true;
                updated++;
                continue;
            }

            if (productCode.Length == 0)
            {
                productCode = await ProductCodeGenerator.GenerateNextAsync(db, codesInFile);
                codesInFile.Add(productCode);
            }

            var product = new Product
            {
                CatId = category.CateId,
                ProductCode = productCode,
                ProductName = row.ProductName,
                ProductDescription = row.ProductDescription
            };
            db.Products.Add(product);
            productsByCode.Add(productCode, product);
            created++;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new ProductImportResultDto(created, updated, categoriesCreated));
    }

    private static void WriteHeaders(IXLWorksheet sheet)
    {
        for (var index = 0; index < Headers.Length; index++)
            sheet.Cell(1, index + 1).Value = Headers[index];

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.SheetView.FreezeRows(1);
    }

    private static void SetProductColumnWidths(IXLWorksheet sheet)
    {
        sheet.Column(1).Width = 24;
        sheet.Column(2).Width = 36;
        sheet.Column(3).Width = 20;
        sheet.Column(4).Width = 32;
        sheet.Column(5).Width = 48;
    }

    private sealed record ImportRow(
        string CategoryName,
        string CategoryDescription,
        string ProductCode,
        string ProductName,
        string ProductDescription
    );
}
