using ProviGo.Common.Models;

namespace TenantService.DTOs
{
    public class BranchUpdateDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
