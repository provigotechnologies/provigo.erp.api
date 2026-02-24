namespace OrderService.DTOs
{
    public class OrderCreateDto
    {
        public Guid BranchId { get; set; }

        public int CustomerId { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public List<OrderItemCreateDto> Items { get; set; } = new();
        public List<OrderDiscountCreateDto>? Discounts { get; set; } = new();
        public List<OrderTaxCreateDto>? Taxes { get; set; } = new();
        public List<OrderChargeCreateDto>? Charges { get; set; } = new();

    }

}
