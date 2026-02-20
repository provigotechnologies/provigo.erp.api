namespace PricingService.DTOs
{
    public class DiscountCreateDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public bool IsActive { get; set; }
    }
}
