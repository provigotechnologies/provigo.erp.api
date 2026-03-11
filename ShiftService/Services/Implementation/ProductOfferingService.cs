using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;
using ShiftService.DTOs;
using ShiftService.Services.Interface;

namespace ShiftService.Services.Implementation
{
    public class ProductOfferingService(
     TenantDbContext db,
     IGenericRepository<ProductOffering> repo,
     BranchAccessService branchAccess,
     TenantProvider tenantProvider,
     CurrentUserService currentUser) : IProductOfferingService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<ProductOffering> _repo = repo;
        private readonly BranchAccessService _branchAccess = branchAccess;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly CurrentUserService _currentUser = currentUser;

        // CREATE OFFERING
        public async Task<ApiResponse<string>> CreateProductOfferingAsync(
            ProductOfferingCreateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                var tenantId = _tenantProvider.TenantId;

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                // Validate TrainerCourse
                var mapping = await _db.TrainerProducts
                    .FirstOrDefaultAsync(x =>
                        x.TrainerProductId == dto.TrainerProductId &&
                        x.TenantId == tenantId &&
                        x.BranchId == dto.BranchId &&
                        x.IsActive);

                if (mapping == null)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid trainer-course mapping");

                // Validate Shift
                var shiftExists = await _db.Shifts
                    .AnyAsync(s =>
                        s.ShiftId == dto.ShiftId &&
                        s.IsActive);

                if (!shiftExists)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid shift");

                // Validate Time
                if (dto.EndTime <= dto.StartTime)
                    return ApiResponseFactory.Failure<string>(
                        "End time must be greater than start time");

                // Prevent trainer overlap
                var overlap = await _db.ProductOfferings
                    .Include(o => o.TrainerCourse)
                    .AnyAsync(o =>
                        o.TrainerCourse.TrainerId == mapping.TrainerId &&
                        o.BranchId == dto.BranchId &&
                        o.TenantId == tenantId &&
                        o.IsActive &&
                        dto.StartTime < o.EndTime &&
                        dto.EndTime > o.StartTime);

                if (overlap)
                    return ApiResponseFactory.Failure<string>(
                        "Trainer already assigned during this time");

                var offering = new ProductOffering
                {
                    TenantId = tenantId,
                    BranchId = dto.BranchId,
                    TrainerCourseId = dto.TrainerProductId,
                    ShiftId = dto.ShiftId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsActive = true
                };

                _db.ProductOfferings.Add(offering);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    "Course offering created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        // GET OFFERINGS
        public async Task<ApiResponse<List<ProductOffering>>> GetOfferingsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.ProductOfferings
                    .Where(s => allowedBranches.Contains(s.BranchId))
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    s => includeInactive || s.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Product Offering fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<ProductOffering>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // UPDATE OFFERING
        public async Task<ApiResponse<string>> UpdateProductOfferingAsync(
     int offeringId,
     ProductOfferingUpdateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var offering = await _db.ProductOfferings
                    .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

                if (offering == null)
                    return ApiResponseFactory.Failure<string>(
                        "Course offering not found");

                if (!allowedBranches.Contains(offering.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                var shiftExists = await _db.Shifts
                .AnyAsync(s => s.ShiftId == dto.ShiftId && s.IsActive);

                if (!shiftExists)
                    return ApiResponseFactory.Failure<string>("Invalid shift");

                if (dto.EndTime <= dto.StartTime)
                    return ApiResponseFactory.Failure<string>(
                        "End time must be greater than start time");

                int affectedRows = await _db.ProductOfferings
                    .Where(o => o.OfferingId == offeringId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(o => o.StartTime, dto.StartTime)
                        .SetProperty(o => o.EndTime, dto.EndTime)
                        .SetProperty(o => o.ShiftId, dto.ShiftId)
                        .SetProperty(o => o.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Update failed");

                return ApiResponseFactory.Success(
                    "Course offering updated successfully");
            }
            catch (Exception)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred");
            }
        }


        // DELETE OFFERING 
        public async Task<ApiResponse<string>> DeleteProductOfferingAsync(
     int offeringId)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var offering = await _db.ProductOfferings
                    .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

                if (offering == null)
                    return ApiResponseFactory.Failure<string>(
                        "Course offering not found");

                if (!allowedBranches.Contains(offering.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                _db.ProductOfferings.Remove(offering);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    $"Course offering {offeringId} deleted successfully");
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