using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OneSource.Api.Models
{
    public class ProductCategory : IAuditableEntity
    {
        [Key]
        public int CateId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CatName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
