namespace PricingService.DTOs
{
    public class TaxCreateDto
    {
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }

    }
}
