namespace PricingService.DTOs
{
    public class TaxResponseDto
    {
        public int TaxId { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Rate { get; set; }
        public bool IsActive { get; set; }

    }

}
