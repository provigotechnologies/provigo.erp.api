using IdentityService.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using AttendanceService.DTOs;

namespace AttendanceService.Services.Interface
{
    public interface IAttendanceService
    {
        Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceAsync(
       PaginationRequest request,
       bool includeInactive);
        Task<ApiResponse<AttendanceResponseDto>> CreateAttendanceAsync(AttendanceDto dto);

    }
}
