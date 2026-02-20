namespace PaymentService.DTOs
{
    public class RefundDto
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
