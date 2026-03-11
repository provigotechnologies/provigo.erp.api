using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using TenantService.DTOs;

namespace TenantService.Services.Interface
{
    public interface IBranchService
    {
        Task<ApiResponse<List<Branch>>> GetBranchesAsync(
            PaginationRequest request, 
            bool includeInactive);

        Task<ApiResponse<BranchResponseDto>> CreateBranchAsync(
            BranchCreateDto dto);

        Task<ApiResponse<string>> UpdateBranchAsync(
            Guid branchId,
            BranchUpdateDto dto);

        Task<ApiResponse<string>> RemoveBranchAsync(Guid branchId);

        Task<ApiResponse<BranchResponseDto>> GetBranchByIdAsync(Guid branchId);

        Task<List<StateDropdownDto>> GetStateDropdownAsync();

    }
}
