using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        public int InstituteId { get; set; }

        [ForeignKey(nameof(InstituteId))]
        public Institute Institute { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
