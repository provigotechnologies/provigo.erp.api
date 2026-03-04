namespace PaymentService.DTOs
{
    public class PaymentCreateDto
    {
        public int OrderId { get; set; }
        public string Mode { get; set; }        
        public decimal PaidAmount { get; set; }
    }
}
