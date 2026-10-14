using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.Models;

public class HomeContent : IAuditableEntity
{
    [Key]
    public int HomeContentId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Heading { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
