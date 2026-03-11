using ProviGo.Common.Models;

namespace TenantService.DTOs
{
    public class BranchUpdateDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
