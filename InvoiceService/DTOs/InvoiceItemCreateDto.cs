namespace InvoiceService.DTOs
{
    public class InvoiceItemCreateDto
    {
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GSTPercent { get; set; }

    }
}
