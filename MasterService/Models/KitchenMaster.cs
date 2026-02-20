using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterService.Models
{
    public class KitchenMaster
    {
        public int KitchenId { get; set; }
        public int KotPrinterId { get; set; }
        public int KotPrinterSizeId { get; set; }
        public int PrintParamId { get; set; }

        public string KitchenName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("KotPrinterId")]
        public KotPrinter KotPrinter { get; set; } = default!;

        [ForeignKey("KotPrinterSizeId")]
        public KotPrinterSize KotPrinterSize { get; set; } = default!;

        [ForeignKey("PrintParamId")]
        public PrintParam PrintParam { get; set; } = default!;
    }
}
