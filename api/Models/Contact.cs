using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OneSource.Api.Models
{
    public class Contact : IAuditableEntity
    {
        [Key]
        public int ContactId { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? WhatsApp { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(20)]
        public string? EnquiryWhatsAppNumber { get; set; }

        public ICollection<ContactDetail> Details { get; set; } = new List<ContactDetail>();

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
