using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayService.Models
{
    public class OrderRecord
    {
        [Key]
        public int Id { get; set; }

        public string? OrderId { get; set; }            // razorpay order id
        public long? Amount { get; set; }               // in paise
        public string? Currency { get; set; } = "INR";
        public string? Receipt { get; set; }

        public string? PaymentId { get; set; }          // razorpay payment id (when completed)
        public string? Signature { get; set; }          // razorpay signature
        public string? Status { get; set; }             // created/paid/failed

        public string? CustomerName { get; set; }       // optional metadata
        public string? CustomerEmail { get; set; }      // optional metadata
        public string? CustomerContact { get; set; }    // optional metadata

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
