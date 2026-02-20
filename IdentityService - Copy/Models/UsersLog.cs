using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Models
{
    public class UsersLog
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }  // ✅ Guid to match User.Id

        public string EventMessage { get; set; } = default!;

        public DateTime EventTime { get; set; } = default!;

        // ✅ Optional: Add navigation if needed
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
