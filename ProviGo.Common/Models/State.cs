using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("States")]
    public class State
    {
        [Key]
        public int StateId { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string StateCode { get; set; } = string.Empty;

        [ForeignKey(nameof(CountryId))]
        public Country Country { get; set; }
    }
}