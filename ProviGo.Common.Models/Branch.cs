using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Branches")]
    public class Branch
    {
        [Key]
        public Guid BranchId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BranchName { get; set; }

        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        // 🔗 One Branch → Many Orders
        public ICollection<Order> Orders { get; set; }
    }
}
