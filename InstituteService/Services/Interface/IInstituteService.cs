using InstituteService.Data;
using InstituteService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace InstituteService.Services.Interface
{
    public interface IInstituteService
    {
        Task<ApiResponse<List<TenantDetails>>> GetInstitutesAsync(
        PaginationRequest request,
        bool includeInactive);

        Task<ApiResponse<string>> UpdateInstituteAsync( Guid instituteId, InstituteDto dto);

        Task<ApiResponse<InstituteResponseDto>> CreateInstituteAsync(InstituteDto dto);
        Task<ApiResponse<string>> RemoveInstituteAsync(Guid instituteId);

    }
}
