using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        [Required, MaxLength(100)]
        public string CustomerName { get; set; }

        [MaxLength(10)]
        public string Phone { get; set; }

        [MaxLength(250)]
        public string Email { get; set; }
        public string State { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(15)]
        public string GSTIN { get; set; } = string.Empty;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }
    }

}
