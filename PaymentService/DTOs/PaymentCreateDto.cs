namespace PaymentService.DTOs
{
    public class PaymentCreateDto
    {
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        public int OrderId { get; set; }
        public decimal TotalPayable { get; set; }

    }
}
