namespace PricingService.DTOs
{
    public class ChargeDto
    {
        public int ChargeId { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Flat | Percentage
        public string ChargeType { get; set; } = string.Empty;

        public decimal Value { get; set; }
        public bool IsActive { get; set; }

    }

}
