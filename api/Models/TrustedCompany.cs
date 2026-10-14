using System;
using System.ComponentModel.DataAnnotations;

namespace OneSource.Api.Models
{
    public class TrustedCompany : IAuditableEntity
    {
        [Key]
        public int TrustedCompanyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
