using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string FullName { get; set; }

        [MaxLength(10)]
        public string Phone { get; set; }

        [MaxLength(250)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }
    }

}
