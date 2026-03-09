using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Plans")]
    public class Plan
    {
        [Key]
        public int PlanId { get; set; }

        [Required, MaxLength(50)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public int MaxStudents { get; set; }

        public bool IsActive { get; set; } = true;

    }

}