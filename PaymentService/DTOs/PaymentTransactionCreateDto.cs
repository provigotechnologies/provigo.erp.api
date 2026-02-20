namespace PaymentService.DTOs
{
    public class PaymentTransactionCreateDto
    {
        public string Mode { get; set; }
        public decimal Amount { get; set; }
        public string GatewayRef { get; set; }
    }
}
