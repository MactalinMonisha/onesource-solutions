using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.DTOs;

public sealed record AboutImageDto(int AboutImageId, string ImageUrl);

public sealed record AboutPageDto(string Heading, string Description, IEnumerable<AboutImageDto> Images);

public sealed record UpdateAboutPageDto
{
    [Required]
    [MaxLength(250)]
    public string Heading { get; init; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; init; } = string.Empty;
}
