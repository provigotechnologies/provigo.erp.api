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

        public async Task<ApiResponse<AttendanceDto>> CreateAttendanceRecordAsync(
     AttendanceCreateDto dto, Guid tenantId)
        {
            try
            {
                // ✅ 1. Check Customer exists & active
                var customer = await _db.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.CustomerId == dto.CustomerId &&
                        c.TenantId == tenantId);

                if (customer == null)
                    return ApiResponseFactory.Failure<AttendanceDto>(
                        "Customer not found");

                if (!customer.IsActive)
                    return ApiResponseFactory.Failure<AttendanceDto>(
                        "Inactive customer. Attendance not allowed");

                // 2️⃣ Validate Shift
                var shift = await _db.Shifts
                    .FirstOrDefaultAsync(s =>
                        s.ShiftId == dto.ShiftId &&
                        s.TenantId == tenantId &&
                        s.IsActive);

                if (shift == null)
                    return ApiResponseFactory.Failure<AttendanceDto>(
                        "Invalid shift");

                // ✅ 2. Duplicate attendance check (same day)
                var attendanceDate = dto.AttendanceDate.Date;

                bool alreadyMarked = await _db.AttendanceRecords.AnyAsync(a =>
                    a.CustomerId == dto.CustomerId &&
                    a.ShiftId == dto.ShiftId &&
                    a.AttendanceDate.Date == attendanceDate);

                if (alreadyMarked)
                {
                    return ApiResponseFactory.Failure<AttendanceDto>(
                        "Attendance already marked for this date");
                }

                // ✅ 3. Create attendance
                var attendance = new AttendanceRecord
                {
                    TenantId = tenantId,
                    CustomerId = dto.CustomerId,
                    ShiftId = dto.ShiftId,
                    AttendanceDate = attendanceDate,
                    IsPresent = dto.IsPresent,
                    IsActive = true
                };

                _db.AttendanceRecords.Add(attendance);
                await _db.SaveChangesAsync();

                var responseDto = new AttendanceDto
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
                return ApiResponseFactory.Failure<AttendanceDto>(
                    "Database error occurred");
            }
        }


        public async Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceRecordsAsync(
        PaginationRequest request,
        bool includeInactive,
        Guid tenantId)
        {
            try
            {
                // ✅ 1️⃣ Tenant Isolation 
                var query = _db.AttendanceRecords
                    .AsNoTracking()
                    .Where(a => a.TenantId == tenantId);

                // ✅ 2️⃣ Apply Active Filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Attendance fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<AttendanceRecord>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<string>> UpdateAttendanceRecordAsync(int attendanceId, AttendanceUpdateDto dto, Guid tenantId)
        {
            try
            {
                int affectedRows = await _db.AttendanceRecords
                   .Where(p => p.AttendanceId == attendanceId
                            && p.TenantId == tenantId)
                   .ExecuteUpdateAsync(s => s
                       .SetProperty(i => i.IsPresent, dto.IsPresent)
                   );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Attendance not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Attendance updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }



    }
}