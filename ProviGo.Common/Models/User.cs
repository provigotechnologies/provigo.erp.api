using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Users")]

    public class User 
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(100)]
        public Guid TenantId { get; set; }

        public string Email { get; set; } = default!;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = default!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [MaxLength(10)]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        public int RoleId { get; set; }

        public string UserCategory { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }

        // One User → Many Logs
        public ICollection<UsersLog> UsersLogs { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(RoleId))]
        public UserRole UserRole { get; set; }

        public ICollection<UserBranch> UserBranches { get; set; }
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    }

}
