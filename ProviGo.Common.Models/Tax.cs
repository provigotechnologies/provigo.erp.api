using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Taxes")]
    public class Tax
    {
        [Key]
        public int TaxId { get; set; }

        [Required]
        public Guid TenantId { get; set; }   

        [Required, MaxLength(50)]
        public string Name { get; set; }     // GST, VAT

        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        public ICollection<OrderTax> OrderTaxes { get; set; }
    }
}
