using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Discounts")]
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(20)]
        public string Type { get; set; } // Percentage / Flat

        public decimal Value { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        public ICollection<OrderDiscount> OrderDiscounts { get; set; }
    }
}