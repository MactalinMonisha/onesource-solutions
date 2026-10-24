using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.Models;

public class ContactDetail : IAuditableEntity
{
    [Key]
    public int ContactDetailId { get; set; }

    public int ContactId { get; set; }

    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Type { get; set; } = "text";

    public bool IsEnquiry { get; set; }

    public int SortOrder { get; set; }

    public Contact Contact { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
