using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrustedCompaniesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrustedCompanyDto>>> GetAll()
    {
        var companies = await db.TrustedCompanies
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new TrustedCompanyDto(c.TrustedCompanyId, c.CompanyName, c.IsActive))
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

        var result = new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.IsActive);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrustedCompanyDto>> Update(int id, UpdateTrustedCompanyDto dto)
    {
        var company = await db.TrustedCompanies.FindAsync(id);
        if (company is null || !company.IsActive) return NotFound();

        company.CompanyName = dto.CompanyName;
        await db.SaveChangesAsync();

        return Ok(new TrustedCompanyDto(company.TrustedCompanyId, company.CompanyName, company.IsActive));
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
}
