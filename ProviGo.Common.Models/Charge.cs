using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Charges")]
    public class Charge
    {
        [Key]
        public int ChargeId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } // Service Fee, Packing

        [Required, MaxLength(20)]
        public string ChargeType { get; set; } // Flat / Percentage

        public decimal Value { get; set; }
        public bool IsActive { get; set; }


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        public ICollection<OrderCharge> OrderCharges { get; set; }
    }
}
