using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("PaymentTransactions")]
    public class PaymentTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [Required, MaxLength(20)]
        public string Mode { get; set; } // Cash, Card, UPI

        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string GatewayRef { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } // Success, Failed

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }
    }
}
