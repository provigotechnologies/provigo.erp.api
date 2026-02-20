using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("UserRoles")]
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } = "Admin";

        // One Role → Many Users
        public ICollection<User> Users { get; set; }
    }
}
