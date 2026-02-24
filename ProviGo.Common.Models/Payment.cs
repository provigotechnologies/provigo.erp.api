using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProviGo.Common.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        public int OrderId { get; set; }
        public string Mode { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public string Status { get; set; } // Pending / Success / Failed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Navigation
        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        public ICollection<PaymentTransaction> Transactions { get; set; }
        public ICollection<Refund> Refunds { get; set; }
    }

}
