using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] int? categoryId)
    {
        var query = db.Products.Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CatId == categoryId.Value);

        var products = await query
            .Include(p => p.Category)
            .OrderBy(p => p.CatId)
            .ThenBy(p => p.ProductId)
            .Select(p => new ProductDto(p.ProductId, p.ProductName, p.ProductDescription, p.CatId, p.Category!.CatName, p.IsActive))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null || !product.IsActive) return NotFound();
        return Ok(new ProductDto(product.ProductId, product.ProductName, product.ProductDescription, product.CatId, product.Category?.CatName, product.IsActive));
    }

    [HttpGet("by-category/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<ProductWithImagesDto>>> GetByCategory(int categoryId)
    {
        var query = db.Products.Where(p => p.IsActive);

        if (categoryId != 0)
        {
            var categoryExists = await db.ProductCategories.AnyAsync(c => c.CateId == categoryId);
            if (!categoryExists) return NotFound($"Product category {categoryId} was not found.");

            query = query.Where(p => p.CatId == categoryId);
        }

        var products = await query
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .OrderBy(p => p.CatId)
            .ThenBy(p => p.ProductId)
            .Select(p => new ProductWithImagesDto(
                p.ProductId,
                p.ProductName,
                p.ProductDescription,
                p.CatId,
                p.Category!.CatName,
                p.IsActive,
                p.ProductImages
                    .Where(pi => pi.IsActive)
                    .Select(pi => new ProductImageDto(pi.ProductImageId, pi.ImageUrl, pi.IsActive))))
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var product = new Product
        {
            ProductName = dto.ProductName,
            ProductDescription = dto.ProductDescription,
            CatId = dto.CatId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var result = new ProductDto(product.ProductId, product.ProductName, product.ProductDescription, product.CatId, null, product.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, result);
    }

    [HttpPost("with-images")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductWithImagesDto>> CreateWithImages(CreateProductWithImagesDto dto)
    {
        var categoryExists = await db.ProductCategories.AnyAsync(c => c.CateId == dto.CatId);
        if (!categoryExists) return NotFound($"Product category {dto.CatId} was not found.");

        var product = new Product
        {
            ProductName = dto.ProductName,
            ProductDescription = dto.ProductDescription,
            CatId = dto.CatId,
            ProductImages = dto.ImageUrls
                .Select(url => new ProductImage { ImageUrl = url })
                .ToList()
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var result = new ProductWithImagesDto(
            product.ProductId,
            product.ProductName,
            product.ProductDescription,
            product.CatId,
            null,
            product.IsActive,
            product.ProductImages.Select(pi => new ProductImageDto(pi.ProductImageId, pi.ImageUrl, pi.IsActive)));

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, result);
    }

    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductWithImagesDto>> UploadImages(int id, UploadProductImagesDto dto)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null || !product.IsActive) return NotFound($"Product {id} was not found.");

        var newImages = dto.ImageUrls
            .Select(url => new ProductImage { ProductId = product.ProductId, ImageUrl = url })
            .ToList();

        db.ProductImages.AddRange(newImages);
        await db.SaveChangesAsync();

        // product.ProductImages may already include newImages due to EF relationship fixup,
        // so dedupe by ProductImageId instead of blindly concatenating.
        var result = new ProductWithImagesDto(
            product.ProductId,
            product.ProductName,
            product.ProductDescription,
            product.CatId,
            product.Category?.CatName,
            product.IsActive,
            product.ProductImages
                .Concat(newImages)
                .Where(pi => pi.IsActive)
                .DistinctBy(pi => pi.ProductImageId)
                .Select(pi => new ProductImageDto(pi.ProductImageId, pi.ImageUrl, pi.IsActive)));

        return Ok(result);
    }

    [HttpPost("{id:int}/upload-images")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ProductWithImagesDto>> UploadImageFiles(int id, [FromForm] List<IFormFile> files)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null || !product.IsActive) return NotFound($"Product {id} was not found.");

        if (files is null || files.Count == 0) return BadRequest("No files were uploaded.");

        var categoryFolder = SanitizeForPath(product.Category?.CatName ?? "uncategorized");
        var productFolder = SanitizeForPath($"{product.ProductId}_{product.ProductName}");

        var webRootPath = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : env.WebRootPath;

        var targetDirectory = Path.Combine(webRootPath, "product-images", categoryFolder, productFolder);
        Directory.CreateDirectory(targetDirectory);

        var newImages = new List<ProductImage>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetDirectory, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/product-images/{categoryFolder}/{productFolder}/{fileName}".Replace("\\", "/");
            newImages.Add(new ProductImage { ProductId = product.ProductId, ImageUrl = relativeUrl });
        }

        db.ProductImages.AddRange(newImages);
        await db.SaveChangesAsync();

        // product.ProductImages may already include newImages due to EF relationship fixup,
        // so dedupe by ProductImageId instead of blindly concatenating.
        var result = new ProductWithImagesDto(
            product.ProductId,
            product.ProductName,
            product.ProductDescription,
            product.CatId,
            product.Category?.CatName,
            product.IsActive,
            product.ProductImages
                .Concat(newImages)
                .Where(pi => pi.IsActive)
                .DistinctBy(pi => pi.ProductImageId)
                .Select(pi => new ProductImageDto(pi.ProductImageId, pi.ImageUrl, pi.IsActive)));

        return Ok(result);
    }

    private static string SanitizeForPath(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalidChars.Contains(c) || c == ' ' ? '-' : c).ToArray());
        return cleaned.Trim('-');
    }

    [HttpDelete("images/{imageId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var image = await db.ProductImages.FindAsync(imageId);
        if (image is null || !image.IsActive) return NotFound();

        image.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.ProductName = dto.ProductName;
        product.ProductDescription = dto.ProductDescription;
        product.CatId = dto.CatId;
        product.IsActive = dto.IsActive;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return NoContent();
    }
}
