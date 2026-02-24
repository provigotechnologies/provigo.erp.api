namespace PaymentService.DTOs
{
    public class OrderPaymentHistoryDto
    {
        public int PaymentId { get; set; }                // Payment table Id
        public int? TransactionId { get; set; }          // Transaction table Id (only for online)
        public decimal Amount { get; set; }              // Paid amount
        public string Mode { get; set; }                 // OFFLINE, ONLINE, Razorpay, etc.
        public string Status { get; set; }               // Paid, Partial, Pending
        public DateTime PaidAt { get; set; }             // Date of payment
    }
}
