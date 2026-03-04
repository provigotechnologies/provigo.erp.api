using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("StudentCourseEnrollments")]
    public class StudentCourseEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        public int CustomerId { get; set; }

        public int OfferingId { get; set; }

        public DateTime JoinDate { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [ForeignKey(nameof(OfferingId))]
        public CourseOffering Offering { get; set; }
    }
}
