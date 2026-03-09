using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("TrainerCourses")]
    public class TrainerCourse
    {
        [Key]
        public int TrainerCourseId { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public Guid TrainerId { get; set; }
        public int ProductId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(TrainerId))]
        public User Trainer { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}