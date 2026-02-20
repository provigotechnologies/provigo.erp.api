namespace ProductService.DTOs
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; } = "";
        public decimal TotalFee { get; set; }
        public bool IsActive { get; set; }
    }
}
