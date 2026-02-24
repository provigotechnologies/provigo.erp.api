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

            // 🔥 FK to Payment
            [Required]
            public int PaymentId { get; set; }

            // 🔥 Your internal order reference 
            public int OrderId { get; set; }

            // 🔥 Razorpay order id
            public string? GatewayOrderId { get; set; }

            public string? GatewayPaymentId { get; set; }

            public decimal Amount { get; set; }  // paise

            public string Currency { get; set; } = "INR";

            [Required, MaxLength(20)]
            public string Mode { get; set; }  // ONLINE / CASH / UPI

            [MaxLength(20)]
            public string Status { get; set; }  // Created / Success / Failed

            public string? Signature { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? PaidAt { get; set; }

            // 🔗 Navigation
            [ForeignKey(nameof(PaymentId))]
            public Payment Payment { get; set; }
        }

    }

