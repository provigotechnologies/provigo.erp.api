namespace PaymentService.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public string PaymentStatus { get; set; }
        public string Mode { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PaymentTransactionResponseDto> Transactions { get; set; }
        public List<RefundResponseDto> Refunds { get; set; }
    }
}
