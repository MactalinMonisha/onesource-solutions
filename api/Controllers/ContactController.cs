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
    private static readonly HashSet<string> SupportedTypes = ["phone", "whatsapp", "email", "address", "website", "link", "text"];

    [HttpGet]
    public async Task<ActionResult<ContactDto>> Get()
    {
        var contact = await db.Contacts.Include(c => c.Details).FirstOrDefaultAsync();
        if (contact is null)
        {
            contact = new Contact();
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
        }

        if (contact.Details.Count == 0)
        {
            var existingDetails = new[]
            {
                new ContactDetail { Label = "Phone", Value = contact.Phone ?? string.Empty, Type = "phone", SortOrder = 0 },
                new ContactDetail { Label = "WhatsApp", Value = contact.WhatsApp ?? string.Empty, Type = "whatsapp", SortOrder = 1, IsEnquiry = true },
                new ContactDetail { Label = "Email", Value = contact.Email ?? string.Empty, Type = "email", SortOrder = 2 },
                new ContactDetail { Label = "Location", Value = contact.Location ?? string.Empty, Type = "address", SortOrder = 3 },
                new ContactDetail { Label = "Website", Value = contact.Website ?? string.Empty, Type = "website", SortOrder = 4 },
            }.Where(d => !string.IsNullOrWhiteSpace(d.Value)).ToList();

            if (existingDetails.Count > 0)
            {
                existingDetails.Add(new ContactDetail
                {
                    Label = "Postal Address",
                    Value = "P.O. Box 221 – 00623, Nairobi, Kenya",
                    Type = "text",
                    SortOrder = existingDetails.Count
                });
                contact.Details = existingDetails;
                await db.SaveChangesAsync();
            }
        }

        return Ok(ToDto(contact));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ContactDto>> Update(UpdateContactDto dto)
    {
        var contact = await db.Contacts.Include(c => c.Details).FirstOrDefaultAsync();
        if (contact is null)
        {
            contact = new Contact();
            db.Contacts.Add(contact);
        }

        var details = dto.Details;
        if (details is null)
        {
            details = new[]
            {
                new UpdateContactDetailDto("Phone", dto.Phone ?? string.Empty, "phone"),
                new UpdateContactDetailDto("WhatsApp", dto.WhatsApp ?? string.Empty, "whatsapp", true),
                new UpdateContactDetailDto("Email", dto.Email ?? string.Empty, "email"),
                new UpdateContactDetailDto("Location", dto.Location ?? string.Empty, "address"),
                new UpdateContactDetailDto("Website", dto.Website ?? string.Empty, "website"),
            }.Where(d => !string.IsNullOrWhiteSpace(d.Value))
             .Concat(contact.Details
                 .Where(d => d.Type is "link" or "text")
                 .Select(d => new UpdateContactDetailDto(d.Label, d.Value, d.Type, d.IsEnquiry)))
             .ToArray();
        }

        if (details.Count > 50)
            return BadRequest("A maximum of 50 contact details can be saved.");

        if (details.Count(d => d?.IsEnquiry == true) > 1)
            return BadRequest("Select only one contact detail for product enquiries.");

        var invalidDetail = details.FirstOrDefault(d =>
            d is null
            || string.IsNullOrWhiteSpace(d.Label)
            || d.Label.Length > 100
            || string.IsNullOrWhiteSpace(d.Value)
            || d.Value.Length > 500
            || !SupportedTypes.Contains(d.Type?.Trim().ToLowerInvariant() ?? string.Empty)
            || (d.Type is not null
                && d.Type.Trim().ToLowerInvariant() is "phone" or "whatsapp"
                && d.Value.Length > 20)
            || (d.Type is not null
                && d.Type.Trim().Equals("email", StringComparison.OrdinalIgnoreCase)
                && d.Value.Length > 255)
            || (d.Type is not null
                && (d.Type.Equals("link", StringComparison.OrdinalIgnoreCase)
                    || d.Type.Equals("website", StringComparison.OrdinalIgnoreCase))
                && !IsHttpUrl(d.Value)));
        if (invalidDetail is not null)
            return BadRequest("Each contact detail must have a label, value, and supported type. Links must use an http or https URL.");

        var selectedEnquiryDetail = details.FirstOrDefault(d => d.IsEnquiry);
        if (selectedEnquiryDetail is not null
            && selectedEnquiryDetail.Type.Trim().ToLowerInvariant() is not ("phone" or "whatsapp" or "email"))
            return BadRequest("Product enquiries can use a phone, WhatsApp, or email contact.");

        var submittedWhatsApp = details.FirstOrDefault(d => d.Type.Trim().Equals("whatsapp", StringComparison.OrdinalIgnoreCase))?.Value.Trim();
        contact.EnquiryWhatsAppNumber = submittedWhatsApp
            ?? contact.Details.FirstOrDefault(d => d.Type == "whatsapp")?.Value
            ?? contact.WhatsApp
            ?? contact.EnquiryWhatsAppNumber;

        db.ContactDetails.RemoveRange(contact.Details);
        contact.Details = details
            .Select((detail, index) => new ContactDetail
            {
                Label = detail.Label.Trim(),
                Value = detail.Value.Trim(),
                Type = detail.Type.Trim().ToLowerInvariant(),
                SortOrder = index,
                IsEnquiry = detail.IsEnquiry
            })
            .ToList();

        contact.Phone = contact.Details.FirstOrDefault(d => d.Type == "phone")?.Value;
        contact.WhatsApp = contact.Details.FirstOrDefault(d => d.Type == "whatsapp")?.Value;
        contact.Email = contact.Details.FirstOrDefault(d => d.Type == "email")?.Value;
        contact.Location = contact.Details.FirstOrDefault(d => d.Type == "address")?.Value;
        contact.Website = contact.Details.FirstOrDefault(d => d.Type == "website")?.Value;

        await db.SaveChangesAsync();

        return Ok(ToDto(contact));
    }

    private static ContactDto ToDto(Contact contact)
    {
        string? ValueFor(string type, string? legacyValue) =>
            contact.Details.FirstOrDefault(d => d.Type == type)?.Value ?? legacyValue;

        var details = contact.Details
            .OrderBy(d => d.SortOrder)
            .Select(d => new ContactDetailDto(d.ContactDetailId, d.Label, d.Value, d.Type, d.SortOrder, d.IsEnquiry))
            .ToArray();

        return new ContactDto(
            contact.ContactId,
            ValueFor("phone", contact.Phone),
            ValueFor("whatsapp", contact.WhatsApp),
            ValueFor("email", contact.Email),
            ValueFor("address", contact.Location),
            ValueFor("website", contact.Website),
            contact.EnquiryWhatsAppNumber ?? contact.WhatsApp,
            details);
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
}
