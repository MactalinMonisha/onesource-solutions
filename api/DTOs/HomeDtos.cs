using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.DTOs;

public sealed record HomePageDto(string Heading, string Description);

public sealed record UpdateHomePageDto
{
    [Required]
    [MaxLength(250)]
    public string Heading { get; init; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; init; } = string.Empty;
}
