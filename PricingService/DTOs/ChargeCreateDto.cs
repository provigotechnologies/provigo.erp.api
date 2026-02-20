namespace PricingService.DTOs
{
    public class ChargeCreateDto
    {
        public string Name { get; set; }
        public string ChargeType { get; set; }
        public decimal Value { get; set; }
        public bool IsActive { get; set; }

    }
}
