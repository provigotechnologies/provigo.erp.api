namespace ProductService.DTOs
{
    public class ProductCreateDto
    {
        public Guid BranchId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal TotalFee { get; set; }
        public bool IsActive { get; set; }
    }
}
