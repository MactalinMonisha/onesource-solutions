using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OneSource.Api.Models
{
    public class Offer : IAuditableEntity
    {
        [Key]
        public int OfferId { get; set; }

        [Required]
        [MaxLength(200)]
        public string OfferName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
