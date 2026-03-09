using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using AttendanceService.DTOs;

namespace AttendanceService.Services.Interface
{
    public interface IAttendanceService
    {
        Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceRecordsAsync(
           PaginationRequest request,
           bool includeInactive, Guid tenantId);

        Task<ApiResponse<AttendanceDto>> CreateAttendanceRecordAsync(AttendanceCreateDto dto, Guid tenantId);
        Task<ApiResponse<string>> UpdateAttendanceRecordAsync(int attendanceId, AttendanceUpdateDto dto, Guid tenantId);

    }
}
