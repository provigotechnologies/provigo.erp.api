using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProviGo.Common.Models
{
    [Table("Refunds")]
    public class Refund
    {
        public int RefundId { get; set; }
        public Guid TenantId { get; set; }
        public int PaymentId { get; set; }

        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Initiated, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }
    }

}
