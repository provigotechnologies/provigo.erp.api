using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Shifts")]
    public class Shift : BaseEntity
    {
        [Key]
        public int ShiftId { get; set; }

        [Required, MaxLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        [ForeignKey(nameof(TenantId))]
        public TenantDetails Tenant { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }
    }
}
