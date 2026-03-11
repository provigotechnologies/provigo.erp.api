    namespace IdentityService.DTOs
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public int RoleId { get; set; }
        public string UserCategory { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Guid> BranchIds { get; set; } = new List<Guid>();

    }

}
