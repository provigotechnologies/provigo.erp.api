namespace OrderService.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";

        public decimal SubTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public DateTime CreatedAt { get; set; }

        // 🔹 Add These
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderTaxDto> Taxes { get; set; } = new();
        public List<OrderDiscountDto> Discounts { get; set; } = new();
        public List<OrderChargeDto> Charges { get; set; } = new();
    }


}
