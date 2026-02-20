using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [Required]
        public int InstituteId { get; set; }   // FK

        [ForeignKey(nameof(InstituteId))]
        public Institute Institute { get; set; }

        [Required, MaxLength(150)]
        public string CourseName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFee { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
