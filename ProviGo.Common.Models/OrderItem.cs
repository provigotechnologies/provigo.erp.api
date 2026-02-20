using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        //Navigation
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = default!;
    }
}
