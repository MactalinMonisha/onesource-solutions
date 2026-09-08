using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OfferDto>>> GetAll()
    {
        var offers = await db.Offers
            .Where(o => o.IsActive)
            .OrderBy(o => o.OfferId)
            .Select(o => new OfferDto(o.OfferId, o.OfferName, o.Description, o.IsActive))
            .ToListAsync();

        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfferDto>> GetById(int id)
    {
        var offer = await db.Offers.FindAsync(id);
        if (offer is null || !offer.IsActive) return NotFound();
        return Ok(new OfferDto(offer.OfferId, offer.OfferName, offer.Description, offer.IsActive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OfferDto>> Create(CreateOfferDto dto)
    {
        var offer = new Offer
        {
            OfferName = dto.OfferName,
            Description = dto.Description
        };

        db.Offers.Add(offer);
        await db.SaveChangesAsync();

        var result = new OfferDto(offer.OfferId, offer.OfferName, offer.Description, offer.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = offer.OfferId }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OfferDto>> Update(int id, UpdateOfferDto dto)
    {
        var offer = await db.Offers.FindAsync(id);
        if (offer is null || !offer.IsActive) return NotFound();

        offer.OfferName = dto.OfferName;
        offer.Description = dto.Description;
        await db.SaveChangesAsync();

        return Ok(new OfferDto(offer.OfferId, offer.OfferName, offer.Description, offer.IsActive));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await db.Offers.FindAsync(id);
        if (offer is null || !offer.IsActive) return NotFound();

        offer.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return NoContent();
    }
}
