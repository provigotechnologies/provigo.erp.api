using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ProviGo.Common.Services;
using ShiftService.DTOs;
using ShiftService.Services.Interface;

namespace ShiftService.Services.Implementation
{
    public class ShiftService(
        TenantDbContext db,
        IGenericRepository<Shift> repo,
        BranchAccessService branchAccess) : IShiftService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Shift> _repo = repo;
        private readonly BranchAccessService _branchAccess = branchAccess;


        // CREATE SHIFT
        public async Task<ApiResponse<ShiftResponseDto>> CreateShiftAsync(ShiftCreateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                    return ApiResponseFactory.Failure<ShiftResponseDto>(
                        "You don't have access to this branch");

                var exists = await _db.Shifts.AnyAsync(s =>
                    s.BranchId == dto.BranchId &&
                    s.ShiftName.ToLower() == dto.ShiftName.ToLower());

                if (exists)
                    return ApiResponseFactory.Failure<ShiftResponseDto>(
                        "Shift already exists");

                var shift = new Shift
                {
                    BranchId = dto.BranchId,
                    ShiftName = dto.ShiftName.Trim(),
                    IsActive = dto.IsActive
                };

                _db.Shifts.Add(shift);
                await _db.SaveChangesAsync();

                var response = new ShiftResponseDto
                {
                    ShiftId = shift.ShiftId,
                    BranchId = shift.BranchId,
                    ShiftName = shift.ShiftName,
                    IsActive = shift.IsActive
                };

                return ApiResponseFactory.Success(
                    response,
                    "Shift created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<ShiftResponseDto>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // GET SHIFTS
        public async Task<ApiResponse<List<Shift>>> GetShiftsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.Shifts
                    .Where(s => allowedBranches.Contains(s.BranchId))
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    s => includeInactive || s.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Shifts fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Shift>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // UPDATE SHIFT
        public async Task<ApiResponse<string>> UpdateShiftAsync(
            int shiftId,
            ShiftUpdateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var shift = await _db.Shifts
                    .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

                if (shift == null)
                    return ApiResponseFactory.Failure<string>("Shift not found");

                if (!allowedBranches.Contains(shift.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                var exists = await _db.Shifts.AnyAsync(s =>
                    s.ShiftId != shiftId &&
                    s.BranchId == shift.BranchId &&
                    s.ShiftName.ToLower() == dto.ShiftName.ToLower());

                if (exists)
                    return ApiResponseFactory.Failure<string>(
                        "Shift name already exists");

                int affectedRows = await _db.Shifts
                    .Where(s => s.ShiftId == shiftId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(i => i.ShiftName, dto.ShiftName.Trim())
                        .SetProperty(i => i.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Update failed");

                return ApiResponseFactory.Success(
                    "Shift updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // DELETE SHIFT
        public async Task<ApiResponse<string>> RemoveShiftAsync(int shiftId)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var shift = await _db.Shifts
                    .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

                if (shift == null)
                    return ApiResponseFactory.Failure<string>("Shift not found");

                if (!allowedBranches.Contains(shift.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                _db.Shifts.Remove(shift);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    $"Shift {shiftId} deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }
    }
}