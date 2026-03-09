using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("UsersLogs")]
    public class UsersLog
    {
        [Key]
        public int UsersLogId { get; set; }

        [Required]
        public Guid UserId { get; set; }   // FK

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string EventMessage { get; set; } = "";
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
    }
}
