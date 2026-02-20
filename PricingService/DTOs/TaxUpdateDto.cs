namespace PricingService.DTOs
{
    public class TaxUpdateDto
    {
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
    }
}
