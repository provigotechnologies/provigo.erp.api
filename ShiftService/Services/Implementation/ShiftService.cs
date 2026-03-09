using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ShiftService.DTOs;
using ShiftService.Services.Interface;

namespace ShiftService.Services.Implementation
{
    public class ShiftService(
        TenantDbContext db,
        IGenericRepository<Shift> repo) : IShiftService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Shift> _repo = repo;

        public async Task<ApiResponse<ShiftResponseDto>> CreateShiftAsync(ShiftCreateDto dto)
        {
            try
            {
                var exists = await _db.Shifts
                    .AnyAsync(s => s.ShiftName.ToLower() == dto.ShiftName.ToLower());

                if (exists)
                    return ApiResponseFactory.Failure<ShiftResponseDto>(
                        "Shift already exists."
                    );

                var shift = new Shift
                {
                    ShiftName = dto.ShiftName.Trim(),
                    IsActive = dto.IsActive
                };

                _db.Shifts.Add(shift);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(new ShiftResponseDto
                {
                    ShiftId = shift.ShiftId,
                    ShiftName = shift.ShiftName,
                    IsActive = shift.IsActive
                }, "Shift created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<ShiftResponseDto>(
                    "Error creating shift",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ApiResponse<List<Shift>>> GetShiftsAsync(
    PaginationRequest request,
    bool includeInactive)
        {
            try
            {
                var query = _db.Shifts.AsNoTracking();

                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Shifts fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Shift>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<string>> UpdateShiftAsync(
     int shiftId,
     ShiftUpdateDto dto)
        {
            try
            {
                var exists = await _db.Shifts
                    .AnyAsync(s => s.ShiftName.ToLower() == dto.ShiftName.ToLower()
                                && s.ShiftId != shiftId);

                if (exists)
                    return ApiResponseFactory.Failure<string>(
                        "Shift name already exists.");

                int affectedRows = await _db.Shifts
                    .Where(s => s.ShiftId == shiftId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(i => i.ShiftName, dto.ShiftName.Trim())
                        .SetProperty(i => i.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    throw new NotFoundException("Shift not found");

                return ApiResponseFactory.Success("Shift updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Error updating shift",
                    new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<string>> RemoveShiftAsync(int shiftId)
        {
            try
            {
                var shift = await _db.Shifts
                    .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

                if (shift == null)
                    return ApiResponseFactory.Failure<string>("Shift not found");

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


        public async Task<ApiResponse<string>> CreateCourseOfferingAsync(
    CourseOfferingCreateDto dto,
    Guid branchId,
    Guid tenantId)
        {
            try
            {
                // 1️⃣ Validate TrainerCourse Mapping
                var mapping = await _db.TrainerCourses
                    .FirstOrDefaultAsync(x =>
                        x.TrainerCourseId == dto.TrainerCourseId &&
                        x.TenantId == tenantId &&
                        x.BranchId == branchId &&
                        x.IsActive);

                if (mapping == null)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid trainer-course mapping");

                // 2️⃣ Validate Shift
                var shiftExists = await _db.Shifts
                    .AnyAsync(s => s.ShiftId == dto.ShiftId && s.IsActive);

                if (!shiftExists)
                    return ApiResponseFactory.Failure<string>("Invalid shift");

                // 3️⃣ Validate Time Logic
                if (dto.EndTime <= dto.StartTime)
                    return ApiResponseFactory.Failure<string>(
                        "End time must be greater than start time");

                // 4️⃣ Prevent Trainer Overlap (IMPORTANT)
                var overlap = await _db.CourseOfferings
                    .Include(o => o.TrainerCourse)
                    .AnyAsync(o =>
                        o.TrainerCourse.TrainerId == mapping.TrainerId &&
                        o.BranchId == branchId &&
                        o.TenantId == tenantId &&
                        o.IsActive &&
                        (
                            dto.StartTime < o.EndTime &&
                            dto.EndTime > o.StartTime
                        )
                    );

                if (overlap)
                    return ApiResponseFactory.Failure<string>(
                        "Trainer already assigned during this time");

                // 5️⃣ Create Offering
                var offering = new CourseOffering
                {
                    TrainerCourseId = dto.TrainerCourseId,
                    ShiftId = dto.ShiftId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    BranchId = branchId,
                    TenantId = tenantId,
                    IsActive = true
                };

                _db.CourseOfferings.Add(offering);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success("Course offering created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Error creating course offering",
                    new List<string> { ex.Message });
            }
        }

    }

}