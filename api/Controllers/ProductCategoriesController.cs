using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategoryDto>>> GetAll()
    {
        var categories = await db.ProductCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.CatName)
            .Select(c => new ProductCategoryDto(c.CateId, c.CatName, c.Description, c.IsActive))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductCategoryDto>> GetById(int id)
    {
        var category = await db.ProductCategories.FindAsync(id);
        if (category is null || !category.IsActive) return NotFound();
        return Ok(new ProductCategoryDto(category.CateId, category.CatName, category.Description, category.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductCategoryDto>> Create(CreateProductCategoryDto dto)
    {
        var category = new ProductCategory
        {
            CatName = dto.CatName,
            Description = dto.Description
        };

        db.ProductCategories.Add(category);
        await db.SaveChangesAsync();

        var result = new ProductCategoryDto(category.CateId, category.CatName, category.Description, category.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = category.CateId }, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.ProductCategories.FindAsync(id);
        if (category is null || !category.IsActive) return NotFound();

        category.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return NoContent();
    }
}
