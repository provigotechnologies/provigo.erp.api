namespace ProductService.DTOs
{
    public class ProductResponseDto
    {
        public int ProductId { get; set; }            
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        public string ProductName { get; set; } = ""; 
        public decimal TotalFee { get; set; } 
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
