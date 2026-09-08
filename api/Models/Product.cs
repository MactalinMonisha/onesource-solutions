using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OneSource.Api.Models
{
    public class Product : IAuditableEntity
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int CatId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ProductDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CatId))]
        public ProductCategory? Category { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
