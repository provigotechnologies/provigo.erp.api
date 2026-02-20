namespace PaymentService.DTOs
{
    public class PaymentTransactionDto
    {
        public int TransactionId { get; set; }
        public int PaymentId { get; set; }
        public string Mode { get; set; }
        public decimal Amount { get; set; }
        public string GatewayRef { get; set; }
        public string Status { get; set; }
    }
}
