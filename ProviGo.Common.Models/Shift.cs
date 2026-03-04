using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Shifts")]
    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }

        [Required, MaxLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
