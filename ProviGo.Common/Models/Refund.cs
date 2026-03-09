using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Refunds")]
    public class Refund
    {
        public int RefundId { get; set; }
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string Mode { get; set; } = "OFFLINE";

        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Initiated, Completed

        public string? GatewayRefundId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }
    }

}
