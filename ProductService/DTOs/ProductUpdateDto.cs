namespace ProductService.DTOs
{
    public class ProductUpdateDto
    {
        public string ProductName { get; set; } = "";
        public decimal TotalFee { get; set; }         
        public bool IsActive { get; set; }            
    }
}
