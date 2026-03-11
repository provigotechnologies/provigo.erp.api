namespace IdentityService.DTOs
{
    public class UserCreateRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public int RoleId { get; set; }
        public string UserCategory { get; set; } = default!;   
        public bool IsActive { get; set; }
        public List<Guid> BranchIds { get; set; } = new List<Guid>();

    }
}
