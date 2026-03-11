using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Refunds")]
    public class Refund : BaseEntity
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string Mode { get; set; } = "OFFLINE";

        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Initiated, Completed

        public string? GatewayRefundId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }
    }

}
