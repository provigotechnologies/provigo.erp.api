using AttendanceService.DTOs;
using AttendanceService.Services.Interface;
using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace AttendanceService.Services.Implementation
{
    public class AttendanceService(
        TenantDbContext db,
        IGenericRepository<AttendanceRecord> repo) : IAttendanceService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<AttendanceRecord> _repo = repo;

        public async Task<ApiResponse<AttendanceResponseDto>> CreateAttendanceAsync(
     AttendanceDto dto)
        {
            try
            {
                // ✅ 1. Check Customer exists & active
                var customer = await _db.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.CustomerId == dto.CustomerId &&
                        c.TenantId == dto.TenantDetailsId);

                if (customer == null)
                    return ApiResponseFactory.Failure<AttendanceResponseDto>(
                        "Customer not found");

                if (!customer.IsActive)
                    return ApiResponseFactory.Failure<AttendanceResponseDto>(
                        "Inactive customer. Attendance not allowed");

                // ✅ 2. Duplicate attendance check (same day)
                var attendanceDate = dto.AttendanceDate.Date;

                bool alreadyMarked = await _db.AttendanceRecords.AnyAsync(a =>
                    a.CustomerId == dto.CustomerId &&
                    a.ShiftId == dto.ShiftId &&
                    a.AttendanceDate.Date == attendanceDate);

                if (alreadyMarked)
                {
                    return ApiResponseFactory.Failure<AttendanceResponseDto>(
                        "Attendance already marked for this date");
                }

                // ✅ 3. Create attendance
                var attendance = new AttendanceRecord
                {
                    TenantId = dto.TenantDetailsId,
                    CustomerId = dto.CustomerId,
                    ShiftId = dto.ShiftId,
                    AttendanceDate = attendanceDate,
                    IsPresent = dto.IsPresent,
                    IsActive = true
                };

                _db.AttendanceRecords.Add(attendance);
                await _db.SaveChangesAsync();

                var responseDto = new AttendanceResponseDto
                {
                    AttendanceId = attendance.AttendanceId,
                    CustomerId = attendance.CustomerId,
                    ShiftId = attendance.ShiftId,
                    AttendanceDate = attendance.AttendanceDate,
                    IsPresent = attendance.IsPresent
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Attendance marked successfully"
                );
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<AttendanceResponseDto>(
                    "Database error occurred");
            }
        }


        public async Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.AttendanceRecords.AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsPresent
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Attendance fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<AttendanceRecord>>(ex.Message, ["Database error occurred"]);

            }
        }


    }
}