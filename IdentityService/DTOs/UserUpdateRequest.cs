namespace IdentityService.DTOs
{
    public class UserUpdateRequest
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }

}
