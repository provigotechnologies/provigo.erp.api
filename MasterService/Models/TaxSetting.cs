using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterService.Models
{
    public class TaxSetting
    {
        public int TaxSettingId { get; set; }

        public int ProductId { get; set; }
        public int GroupId { get; set; }

        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public ProductMaster Product { get; set; } = default!;

        [ForeignKey("GroupId")]
        public GroupMaster Group { get; set; } = default!;
    }
}
