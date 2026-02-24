namespace PaymentService.DTOs
{
    public class RefundCreateDto
    {
        public int PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
        public string Mode { get; set; }

    }
}
