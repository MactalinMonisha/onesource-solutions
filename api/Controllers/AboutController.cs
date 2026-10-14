using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AboutController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private const int AboutContentId = 1;
    private const long MaxImageSize = 10_000_000;
    private const string DefaultHeading = "Technology that helps businesses move forward";
    private const string DefaultDescription = "OneSource Solutions Limited is a Nairobi-based ICT partner helping businesses work smarter through dependable technology. We bring together business hardware, tax-compliant systems, software, networking, and security solutions, with practical guidance from selection through implementation and ongoing support.\n\nOur team works with organizations across retail, hospitality, healthcare, and other sectors to match the right technology to the way each business operates.";

    private static readonly IReadOnlyDictionary<string, string> AllowedImageTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    [HttpGet]
    public async Task<ActionResult<AboutPageDto>> Get()
    {
        var about = await GetOrCreateAboutAsync();
        return Ok(ToDto(about));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AboutPageDto>> Update(UpdateAboutPageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Heading) || string.IsNullOrWhiteSpace(dto.Description))
            return BadRequest("Heading and description are required.");

        var about = await GetOrCreateAboutAsync();
        about.Heading = dto.Heading.Trim();
        about.Description = dto.Description.Trim();
        await db.SaveChangesAsync();

        return Ok(ToDto(about));
    }

    [HttpPost("images")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<AboutPageDto>> UploadImages([FromForm] List<IFormFile> files)
    {
        if (files is null || files.Count == 0)
            return BadRequest("No files were uploaded.");
        if (files.Count > 10)
            return BadRequest("You can upload up to 10 images at a time.");

        foreach (var file in files)
        {
            if (file.Length == 0)
                return BadRequest($"{file.FileName} is empty.");
            if (file.Length > MaxImageSize)
                return BadRequest($"{file.FileName} exceeds the 10 MB limit.");
            if (!AllowedImageTypes.ContainsKey(file.ContentType))
                return BadRequest($"{file.FileName} must be a JPEG, PNG, WebP, or GIF image.");
        }

        var about = await GetOrCreateAboutAsync();
        var webRootPath = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : env.WebRootPath;
        var targetDirectory = Path.Combine(webRootPath, "about-images");
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in files)
        {
            var extension = AllowedImageTypes[file.ContentType];
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetDirectory, fileName);

            await using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            about.Images.Add(new AboutImage
            {
                AboutContentId = about.AboutContentId,
                ImageUrl = $"/about-images/{fileName}"
            });
        }

        await db.SaveChangesAsync();
        return Ok(ToDto(about));
    }

    [HttpDelete("images/{imageId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var image = await db.AboutImages.FirstOrDefaultAsync(i => i.AboutImageId == imageId && i.IsActive);
        if (image is null)
            return NotFound();

        image.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<AboutContent> GetOrCreateAboutAsync()
    {
        var about = await db.AboutContents
            .Include(a => a.Images.Where(i => i.IsActive))
            .FirstOrDefaultAsync(a => a.AboutContentId == AboutContentId);

        if (about is not null)
            return about;

        about = new AboutContent
        {
            AboutContentId = AboutContentId,
            Heading = DefaultHeading,
            Description = DefaultDescription
        };
        db.AboutContents.Add(about);
        await db.SaveChangesAsync();
        return about;
    }

    private static AboutPageDto ToDto(AboutContent about) => new(
        about.Heading,
        about.Description,
        about.Images
            .Where(image => image.IsActive)
            .OrderBy(image => image.AboutImageId)
            .Select(image => new AboutImageDto(image.AboutImageId, image.ImageUrl)));
}
