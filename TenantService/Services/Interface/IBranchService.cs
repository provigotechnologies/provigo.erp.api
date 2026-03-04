using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using TenantService.DTOs;

namespace TenantService.Services.Interface
{
    public interface IBranchService
    {
        Task<ApiResponse<List<Branch>>> GetBranchesAsync(PaginationRequest request, bool includeInactive, Guid tenantId);

        Task<ApiResponse<BranchResponseDto>> CreateBranchAsync(BranchCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> UpdateBranchAsync(Guid branchId, BranchUpdateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveBranchAsync(Guid branchId, Guid tenantId);

        Task<ApiResponse<BranchResponseDto>> GetBranchByIdAsync(Guid branchId, Guid tenantId);

        Task<List<StateDropdownDto>> GetStateDropdownAsync();

    }
}
