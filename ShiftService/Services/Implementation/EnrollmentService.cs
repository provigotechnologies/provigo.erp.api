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
    public class EnrollmentService(
        TenantDbContext db,
        TenantProvider tenantProvider,
        BranchAccessService branchAccess,
        IGenericRepository<CustomerProductEnrollment> repo) : IEnrollmentService
    {
        private readonly TenantDbContext _db = db;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly BranchAccessService _branchAccess = branchAccess;
        private readonly IGenericRepository<CustomerProductEnrollment> _repo = repo;

        // ENROLL STUDENT

        public async Task<ApiResponse<string>> EnrollStudentAsync(
            int offeringId,
            int customerId,
            Guid branchId)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(branchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                // Validate Offering
                var offering = await _db.ProductOfferings
                    .FirstOrDefaultAsync(o =>
                        o.OfferingId == offeringId &&
                        o.TenantId == tenantId &&
                        o.BranchId == branchId &&
                        o.IsActive);

                if (offering == null)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid batch for this branch");

                // Validate Customer
                var customerExists = await _db.Customers
                    .AnyAsync(c =>
                        c.CustomerId == customerId &&
                        c.TenantId == tenantId &&
                        c.BranchId == branchId &&
                        c.IsActive);

                if (!customerExists)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid customer for this branch");

                // Prevent duplicate enrollment
                var alreadyEnrolled = await _db.CustomerProductEnrollments
                    .AnyAsync(e =>
                        e.CustomerId == customerId &&
                        e.OfferingId == offeringId &&
                        e.IsActive);

                if (alreadyEnrolled)
                    return ApiResponseFactory.Failure<string>(
                        "Student already enrolled in this batch");

                var enrollment = new CustomerProductEnrollment
                {
                    CustomerId = customerId,
                    OfferingId = offeringId,
                    JoinDate = DateTime.UtcNow,
                    IsActive = true
                };

                _db.CustomerProductEnrollments.Add(enrollment);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    "Student enrolled successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Enrollment failed",
                    new List<string> { ex.Message });
            }
        }


        // GET STUDENTS BY OFFERING

        public async Task<ApiResponse<List<CustomerProductEnrollment>>> GetEnrollmentsByOfferingAsync(
    PaginationRequest request,
    bool includeInactive)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.CustomerProductEnrollments
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    c => includeInactive || c.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Trainer Product fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<CustomerProductEnrollment>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // REMOVE ENROLLMENT

        public async Task<ApiResponse<string>> RemoveEnrollmentAsync(int enrollmentId)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var enrollment = await _db.CustomerProductEnrollments
                    .Include(e => e.Offering)
                    .FirstOrDefaultAsync(e =>
                        e.EnrollmentId == enrollmentId &&
                        e.Offering.TenantId == tenantId);

                if (enrollment == null)
                    return ApiResponseFactory.Failure<string>(
                        "Enrollment not found");

                if (!allowedBranches.Contains(enrollment.Offering.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                enrollment.IsActive = false;

                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    "Enrollment removed successfully");
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