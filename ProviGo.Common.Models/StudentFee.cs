using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("StudentFees")]
    public class StudentFee
    {
        [Key]
        public int StudentFeeId { get; set; }

        [Required]
        public int InstituteId { get; set; }

        [ForeignKey(nameof(InstituteId))]
        public Institute Institute { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public decimal PaidAmount { get; set; }

        [Required]
        public decimal BalanceAmount { get; set; }

        public bool IsActive { get; set; } = true;

    }

}
