using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Orders")]
    public class Order : BaseEntity
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } // Created, Confirmed, Completed, Cancelled
        public decimal SubTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        // 🔗 Navigation
        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();
        public ICollection<OrderTax> OrderTaxes { get; set; } = new List<OrderTax>();
        public ICollection<OrderCharge> OrderCharges { get; set; } = new List<OrderCharge>();

    }
}
