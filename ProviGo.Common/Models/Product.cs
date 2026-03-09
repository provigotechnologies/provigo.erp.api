using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Products")]
    public class Product
    {
        public int ProductId { get; set; }
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFee { get; set; }

        [MaxLength(20)]
        public string? HSNCode { get; set; }

        public int? TaxId { get; set; }
        public int? DiscountId { get; set; }
        public bool IsTaxInclusive { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(TaxId))]
        public Tax? Tax { get; set; }

        [ForeignKey(nameof(DiscountId))]
        public Discount? Discount { get; set; }
    }
}