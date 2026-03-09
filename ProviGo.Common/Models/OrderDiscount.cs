using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("OrderDiscounts")]
    public class OrderDiscount
    {
        public int OrderDiscountId { get; set; }
        public int OrderId { get; set; }
        public int DiscountId { get; set; }

        public decimal DiscountAmount { get; set; }

        //Navigation
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { get; set; } = default!;
    }

}
