namespace PaymentService.DTOs
{
    public class PaymentTransactionCreateDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; } // rupees
        public string Currency { get; set; } = "INR";
        public string? Receipt { get; set; } = null;
    }
}
