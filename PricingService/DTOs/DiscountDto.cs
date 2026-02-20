namespace PricingService.DTOs
{
    public class DiscountDto
    {
        public int DiscountId { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Percentage | Flat
        public string Type { get; set; } = string.Empty;

        public decimal Value { get; set; }

        public bool IsActive { get; set; }
    }

}
