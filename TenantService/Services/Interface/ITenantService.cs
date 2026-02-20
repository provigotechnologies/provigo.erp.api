using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using TenantService.DTOs;

namespace TenantService.Services.Interface
{
    public interface ITenantService
    {
        Task<ApiResponse<List<TenantDetails>>> GetTenantsAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<TenantDto>> CreateTenantAsync(TenantCreateDto dto);

        Task<ApiResponse<string>> UpdateTenantAsync(Guid tenantId, TenantUpdateDto dto);

        Task<ApiResponse<string>> RemoveTenantAsync(Guid tenantId);
    }
}

