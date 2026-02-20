namespace OrderService.DTOs
{
    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
