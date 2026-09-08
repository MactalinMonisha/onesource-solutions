using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OneSource.Api.Models
{
    public class ProductImage : IAuditableEntity
    {
        [Key]
        public int ProductImageId { get; set; }   

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Navigation property
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
    }
}
