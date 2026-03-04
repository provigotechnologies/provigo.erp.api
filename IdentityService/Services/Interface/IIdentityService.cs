using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using IdentityService.DTOs;

namespace IdentityService.Services.Interface
{
    public interface IIdentityService
    {
        Task<ApiResponse<UserResponse>> RegisterAsync(
            UserCreateRequest dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<object>> LoginAsync(LoginDto dto);

        Task<ApiResponse<List<User>>> GetUsersAsync(
            PaginationRequest request,
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdateUserAsync(
            Guid id,
            UserUpdateRequest dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> DeleteUserAsync(
            Guid id,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<List<object>>> GetRolesAsync();

        Task<ApiResponse<List<object>>> GetLogsAsync();

        Task<ApiResponse<string>> LogoutAsync(LogoutRequest dto);
    }

}
