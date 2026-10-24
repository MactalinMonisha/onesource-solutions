using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.Models;

public class CarouselSlide : IAuditableEntity
{
    [Key]
    public int CarouselSlideId { get; set; }

    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(250)]
    public string AltText { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
