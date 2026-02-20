using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public UserRole? UserRole { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
