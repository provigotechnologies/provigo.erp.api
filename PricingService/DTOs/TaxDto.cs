namespace PricingService.DTOs
{
    public class TaxDto
    {
        public int TaxId { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Rate { get; set; }
        public bool IsActive { get; set; }

    }

}
