using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.Models;

public class AboutContent : IAuditableEntity
{
    [Key]
    public int AboutContentId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Heading { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public ICollection<AboutImage> Images { get; set; } = new List<AboutImage>();
}
