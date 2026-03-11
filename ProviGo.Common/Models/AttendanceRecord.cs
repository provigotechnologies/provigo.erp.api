using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("AttendanceRecords")]
    public class AttendanceRecord : BaseEntity
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public bool IsPresent { get; set; }


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [ForeignKey(nameof(ShiftId))]
        public Shift Shift { get; set; }
    }

}