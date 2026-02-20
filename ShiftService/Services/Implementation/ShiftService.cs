using IdentityService.Data;
using ShiftService.Services.Interface;
using ShiftService.DTOs;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ShiftService.Services.Implementation
{
    public class ShiftService(
        TenantDbContext db,
        IGenericRepository<Shift> repo) : IShiftService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Shift> _repo = repo;

        public async Task<ApiResponse<ShiftDto>> CreateShiftAsync(ShiftCreateDto dto)
        {
            try
            {

                // 1️⃣ Validate Product belongs to tenant
                var productExists = await _db.Products
                    .AnyAsync(p => p.ProductId == dto.ProductId
                                && p.TenantId == dto.TenantId);

                if (!productExists)
                    return ApiResponseFactory.Failure<ShiftDto>(
                        "Invalid product for this tenant."
                    );

                // 2️⃣ Validate Trainer belongs to tenant
                var trainerExists = await _db.Users
                    .AnyAsync(u => u.UserId == dto.TrainerId
                                && u.TenantId == dto.TenantId);

                if (!trainerExists)
                    return ApiResponseFactory.Failure<ShiftDto>(
                        "Invalid trainer for this tenant."
                    );

                // 3️⃣ Case-insensitive duplicate check
                var exists = await _db.Shifts
                    .AnyAsync(s => s.TenantId == dto.TenantId &&
                                   s.ShiftName.ToLower() == dto.ShiftName.ToLower());

                if (exists)
                {
                    return ApiResponseFactory.Failure<ShiftDto>(
                        "Shift already exists for this tenant."
                    );
                }

                // 4️⃣ Create shift
                var shift = new Shift
                {
                    TenantId = dto.TenantId,
                    ProductId = dto.ProductId,
                    TrainerId = dto.TrainerId,
                    ShiftName = dto.ShiftName.Trim(),
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.UtcNow
                };

                _db.Shifts.Add(shift);
                await _db.SaveChangesAsync();

                var responseDto = new ShiftDto
                {
                    ShiftId = shift.ShiftId,
                    TenantId = shift.TenantId,
                    ProductId = shift.ProductId,
                    TrainerId = shift.TrainerId,
                    ShiftName = shift.ShiftName,
                    IsActive = shift.IsActive,
                    CreatedOn = shift.CreatedOn
                };

                return ApiResponseFactory.Success(responseDto, "Shift created successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<ShiftDto>(
                    "Database constraint error occurred."
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<ShiftDto>(
                    "Unexpected error occurred.",
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
                // Base query
                var query = _db.Shifts.AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Shift fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Shift>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdateShiftAsync(int shiftId, ShiftUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Shifts
                .Where(i => i.ShiftId == shiftId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.ProductId, dto.ProductId)
                    .SetProperty(i => i.TrainerId, dto.TrainerId)
                    .SetProperty(i => i.ShiftName, dto.ShiftName)
                    .SetProperty(i => i.IsActive, dto.IsActive)
                );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Shift not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Shift updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }

        public async Task<ApiResponse<string>> RemoveShiftAsync(int shiftId)
        {
            try
            {
                var shift = await _db.Shifts.FindAsync(shiftId);

                if (shift == null)
                {
                    return ApiResponseFactory.Failure<string>("Product not found");
                }

                _db.Shifts.Remove(shift);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Shift {shiftId} deleted successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message }
                );
            }
        }

   

    }

    }
