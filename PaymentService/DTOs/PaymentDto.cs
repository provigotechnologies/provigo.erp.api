namespace PaymentService.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public string PaymentStatus { get; set; }
        public string Mode { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PaymentTransactionDto> Transactions { get; set; }
        public List<RefundDto> Refunds { get; set; }
    }
}
