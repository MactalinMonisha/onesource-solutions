using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrustedCompaniesController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private const long MaxLogoSize = 10_000_000;

    private static readonly IReadOnlyDictionary<string, string> AllowedLogoTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrustedCompanyDto>>> GetAll()
    {
        var companies = await db.TrustedCompanies
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new TrustedCompanyDto(c.TrustedCompanyId, c.CompanyName, c.LogoUrl, c.IsActive))
            .ToListAsync();

        return Ok(companies);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrustedCompanyDto>> Create(CreateTrustedCompanyDto dto)
    {
        var company = new TrustedCompany
        {
            CompanyName = dto.CompanyName
        };

        db.TrustedCompanies.Add(company);
        await db.SaveChangesAsync();

        var result = new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.LogoUrl, company.IsActive);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPost("with-logo")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<TrustedCompanyDto>> CreateWithLogo([FromForm] string companyName, [FromForm] IFormFile? logo)
    {
        if (string.IsNullOrWhiteSpace(companyName)) return BadRequest("Company name is required.");
        if (!TryGetLogoExtension(logo, out var extension, out var error)) return BadRequest(error);

        var company = new TrustedCompany { CompanyName = companyName.Trim() };
        db.TrustedCompanies.Add(company);
        await db.SaveChangesAsync();

        company.LogoUrl = await SaveLogoAsync(logo!, company.TrustedCompanyId, extension);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.LogoUrl, company.IsActive));
    }

    [HttpPost("{id:int}/logo")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<TrustedCompanyDto>> UploadLogo(int id, [FromForm] IFormFile? logo)
    {
        var company = await db.TrustedCompanies.FindAsync(id);
        if (company is null || !company.IsActive) return NotFound();
        if (!TryGetLogoExtension(logo, out var extension, out var error)) return BadRequest(error);

        company.LogoUrl = await SaveLogoAsync(logo!, company.TrustedCompanyId, extension);
        await db.SaveChangesAsync();

        return Ok(new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.LogoUrl, company.IsActive));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrustedCompanyDto>> Update(int id, UpdateTrustedCompanyDto dto)
    {
        var company = await db.TrustedCompanies.FindAsync(id);
        if (company is null || !company.IsActive) return NotFound();

        company.CompanyName = dto.CompanyName;
        await db.SaveChangesAsync();

        return Ok(new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.LogoUrl, company.IsActive));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var company = await db.TrustedCompanies.FindAsync(id);
        if (company is null || !company.IsActive) return NotFound();

        company.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static bool TryGetLogoExtension(IFormFile? file, out string extension, out string error)
    {
        extension = string.Empty;
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "A logo image is required.";
            return false;
        }
        if (file.Length > MaxLogoSize)
        {
            error = "Logo images must be 10 MB or smaller.";
            return false;
        }
        if (!AllowedLogoTypes.TryGetValue(file.ContentType, out extension!))
        {
            error = "Logo must be a JPEG, PNG, WebP, or GIF image.";
            return false;
        }

        return true;
    }

    private async Task<string> SaveLogoAsync(IFormFile file, int companyId, string extension)
    {
        var webRootPath = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : env.WebRootPath;
        var targetDirectory = Path.Combine(webRootPath, "trusted-company-logos", companyId.ToString());
        Directory.CreateDirectory(targetDirectory);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetDirectory, fileName);
        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        return $"/trusted-company-logos/{companyId}/{fileName}";
    }
}
