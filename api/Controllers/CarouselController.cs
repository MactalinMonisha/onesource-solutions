using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarouselController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private const long MaxImageSize = 10_000_000;

    private static readonly IReadOnlyDictionary<string, string> AllowedImageTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CarouselSlideDto>>> Get()
    {
        var slides = await db.CarouselSlides
            .OrderBy(slide => slide.SortOrder)
            .Select(slide => new CarouselSlideDto(
                slide.CarouselSlideId,
                slide.ImageUrl,
                slide.AltText,
                slide.Label,
                slide.Description,
                slide.Title,
                slide.SortOrder))
            .ToListAsync();

        return Ok(slides);
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MaxImageSize)]
    public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile? image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Select an image to upload.");
        if (image.Length > MaxImageSize)
            return BadRequest("Carousel images must be 10 MB or smaller.");
        if (!AllowedImageTypes.TryGetValue(image.ContentType, out var extension))
            return BadRequest("Carousel images must be JPEG, PNG, WebP, or GIF files.");
        if (!await HasValidImageSignatureAsync(image, extension))
            return BadRequest("The selected file contents do not match a supported image format.");

        var webRootPath = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : env.WebRootPath;
        var targetDirectory = Path.Combine(webRootPath, "carousel-images");
        Directory.CreateDirectory(targetDirectory);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetDirectory, fileName);
        await using (var output = new FileStream(filePath, FileMode.CreateNew))
        await using (var input = image.OpenReadStream())
        {
            await input.CopyToAsync(output);
        }

        return Ok(new { imageUrl = $"/carousel-images/{fileName}" });
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<CarouselSlideDto>>> Update(IReadOnlyList<UpdateCarouselSlideDto> slides)
    {
        if (slides.Count is < 1 or > 10)
            return BadRequest("The carousel must contain between 1 and 10 slides.");

        foreach (var slide in slides)
        {
            if (string.IsNullOrWhiteSpace(slide.ImageUrl)
                || slide.ImageUrl.Length > 1000
                || string.IsNullOrWhiteSpace(slide.AltText)
                || slide.AltText.Length > 250
                || string.IsNullOrWhiteSpace(slide.Label)
                || slide.Label.Length > 120
                || string.IsNullOrWhiteSpace(slide.Description)
                || slide.Description.Length > 500
                || string.IsNullOrWhiteSpace(slide.Title)
                || slide.Title.Length > 200
                || (!IsHttpUrl(slide.ImageUrl) && !IsUploadedCarouselImagePath(slide.ImageUrl)))
            {
                return BadRequest("Each slide needs a valid image URL, alt text, label, description, and title.");
            }
        }

        var existingSlides = await db.CarouselSlides.ToListAsync();
        db.CarouselSlides.RemoveRange(existingSlides);
        db.CarouselSlides.AddRange(slides.Select((slide, index) => new CarouselSlide
        {
            ImageUrl = slide.ImageUrl.Trim(),
            AltText = slide.AltText.Trim(),
            Label = slide.Label.Trim(),
            Description = slide.Description.Trim(),
            Title = slide.Title.Trim(),
            SortOrder = index
        }));

        await db.SaveChangesAsync();
        return await Get();
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));

    private static bool IsUploadedCarouselImagePath(string value) =>
        value.StartsWith("/carousel-images/", StringComparison.Ordinal)
        && Guid.TryParseExact(Path.GetFileNameWithoutExtension(value), "D", out _)
        && Path.GetExtension(value) is ".jpg" or ".png" or ".webp" or ".gif";

    private static async Task<bool> HasValidImageSignatureAsync(IFormFile image, string extension)
    {
        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        return extension switch
        {
            ".jpg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8
                && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytesRead >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            ".gif" => bytesRead >= 6
                && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
            _ => false
        };
    }
}
