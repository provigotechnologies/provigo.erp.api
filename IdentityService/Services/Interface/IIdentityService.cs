using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using IdentityService.DTOs;

public interface IIdentityService
{
    Task<ApiResponse<UserResponse>> RegisterAsync(UserCreateRequest dto, List<Guid> branchIds);
    Task<ApiResponse<object>> LoginAsync(LoginDto dto);
    Task<ApiResponse<List<User>>> GetUsersAsync(PaginationRequest request, bool includeInactive);
    Task<ApiResponse<string>> UpdateUserAsync(Guid id, UserUpdateRequest dto);
    Task<ApiResponse<string>> DeleteUserAsync(Guid id);
    Task<ApiResponse<List<object>>> GetRolesAsync();
    Task<ApiResponse<List<object>>> GetLogsAsync();
    Task<ApiResponse<string>> LogoutAsync(LogoutRequest dto);
}