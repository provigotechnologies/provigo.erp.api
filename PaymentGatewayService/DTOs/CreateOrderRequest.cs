namespace PaymentGatewayService.DTOs
{
    public class CreateOrderRequest
    {
        public decimal Amount { get; set; } // rupees
        public string Currency { get; set; } = "INR";
        public string? Receipt { get; set; } = null;

        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerContact { get; set; }
    }
}
