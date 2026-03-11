using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("ProductOfferings")]
    public class ProductOffering 
    {
        [Key]
        public int OfferingId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid BranchId { get; set; }
        public int TrainerCourseId { get; set; }
        public int ShiftId { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }


        [ForeignKey(nameof(TrainerCourseId))]
        public TrainerProduct TrainerCourse { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(ShiftId))]
        public Shift Shift { get; set; }
    }
}
