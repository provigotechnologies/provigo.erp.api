namespace PricingService.DTOs
{
    public class ChargeUpdateDto
    {
        public string Name { get; set; }
        public string ChargeType { get; set; }
        public decimal Value { get; set; }
        public bool IsActive { get; set; }
    }
}
