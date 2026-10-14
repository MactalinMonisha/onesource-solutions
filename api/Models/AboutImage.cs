using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneSource.Api.Models;

public class AboutImage : IAuditableEntity
{
    [Key]
    public int AboutImageId { get; set; }

    [Required]
    public int AboutContentId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    [ForeignKey(nameof(AboutContentId))]
    public AboutContent? AboutContent { get; set; }
}
