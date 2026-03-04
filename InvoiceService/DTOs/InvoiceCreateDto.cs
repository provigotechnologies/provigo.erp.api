namespace InvoiceService.DTOs
{
    public class InvoiceCreateDto
    {
        public Guid BranchId { get; set; }
        public string BusinessName { get; set; } = "";
        public string BusinessAddress { get; set; } = "";
        public string BusinessGST { get; set; } = "";

        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";

        public bool IsGST { get; set; }

        public List<InvoiceItemCreateDto> Items { get; set; } = new();

    }
}
