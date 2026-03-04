using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("OrderCharges")]
    public class OrderCharge
    {
        [Key]
        public int OrderChargeId { get; set; }

        public int OrderId { get; set; }

        public int ChargeId { get; set; }

        public decimal ChargeAmount { get; set; }

        //Navigation
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        [ForeignKey(nameof(ChargeId))]
        public Charge Charge { get; set; } = default!;
    }
}
