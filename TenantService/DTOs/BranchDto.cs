namespace TenantService.DTOs
{
    public class BranchDto
    {
        public Guid BranchId { get; set; }
        public Guid TenantId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
