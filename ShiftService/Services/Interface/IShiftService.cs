using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ShiftService.DTOs;

namespace ShiftService.Services.Interface
{
    public interface IShiftService
    {
        Task<ApiResponse<List<Shift>>> GetShiftsAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<ShiftDto>> CreateShiftAsync(ShiftCreateDto dto);

        Task<ApiResponse<string>> UpdateShiftAsync(int shiftId, ShiftUpdateDto dto);

        Task<ApiResponse<string>> RemoveShiftAsync(int shiftId);
    }
}
