using System;

namespace OneSource.Api.Models
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime UpdatedDate { get; set; }
    }
}
