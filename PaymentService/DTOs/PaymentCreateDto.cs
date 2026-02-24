namespace PaymentService.DTOs
{
    public class PaymentCreateDto
    {
        public Guid BranchId { get; set; }
        public int OrderId { get; set; }
        public string Mode { get; set; }        
        public decimal PaidAmount { get; set; }
    }
}
