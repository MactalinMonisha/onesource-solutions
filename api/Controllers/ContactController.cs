using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ContactDto>> Get()
    {
        var contact = await db.Contacts.FirstOrDefaultAsync();
        if (contact is null)
        {
            // Ensure a row always exists so the admin has something to edit.
            contact = new Contact();
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
        }

        return Ok(new ContactDto(contact.ContactId, contact.Phone, contact.WhatsApp, contact.Email, contact.Location, contact.Website));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ContactDto>> Update(UpdateContactDto dto)
    {
        var contact = await db.Contacts.FirstOrDefaultAsync();
        if (contact is null)
        {
            contact = new Contact();
            db.Contacts.Add(contact);
        }

        contact.Phone = dto.Phone;
        contact.WhatsApp = dto.WhatsApp;
        contact.Email = dto.Email;
        contact.Location = dto.Location;
        contact.Website = dto.Website;

        await db.SaveChangesAsync();

        return Ok(new ContactDto(contact.ContactId, contact.Phone, contact.WhatsApp, contact.Email, contact.Location, contact.Website));
    }
}
