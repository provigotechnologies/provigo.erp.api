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
    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }

        [Required, MaxLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
