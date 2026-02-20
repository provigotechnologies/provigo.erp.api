using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    public class OrderTax
    {
        public int OrderTaxId { get; set; }
        public int OrderId { get; set; }
        public int TaxId { get; set; }

        public decimal TaxAmount { get; set; }


        // 🔗 Navigation
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [ForeignKey(nameof(TaxId))]
        public Tax Tax { get; set; }
    }

}
