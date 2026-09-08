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
}
