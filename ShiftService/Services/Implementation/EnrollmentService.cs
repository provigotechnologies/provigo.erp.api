using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;
using ProviGo.Common.Response;
using ShiftService.DTOs;
using ShiftService.Services.Interface;

namespace ShiftService.Services.Implementation
{
    public class EnrollmentService(
        TenantDbContext db) : IEnrollmentService
    {
        private readonly TenantDbContext _db = db;

        public async Task<ApiResponse<string>> EnrollStudentAsync(
            int offeringId,
            int customerId,
            Guid branchId,
            Guid tenantId)
        {
            try
            {
                // 1️⃣ Validate Offering with Tenant + Branch
                var offering = await _db.CourseOfferings
                    .FirstOrDefaultAsync(o =>
                        o.OfferingId == offeringId &&
                        o.TenantId == tenantId &&
                        o.BranchId == branchId &&
                        o.IsActive);

                if (offering == null)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid batch for this branch");

                // 2️⃣ Validate Customer belongs to same Tenant + Branch
                var customerExists = await _db.Customers
                    .AnyAsync(c =>
                        c.CustomerId == customerId &&
                        c.TenantId == tenantId &&
                        c.BranchId == branchId &&
                        c.IsActive);

                if (!customerExists)
                    return ApiResponseFactory.Failure<string>(
                        "Invalid customer for this branch");

                // 3️⃣ Prevent Duplicate Enrollment
                var alreadyEnrolled = await _db.StudentCourseEnrollments
                    .AnyAsync(e =>
                        e.CustomerId == customerId &&
                        e.OfferingId == offeringId &&
                        e.IsActive);

                if (alreadyEnrolled)
                    return ApiResponseFactory.Failure<string>(
                        "Student already enrolled in this batch");

                var enrollment = new StudentCourseEnrollment
                {
                    CustomerId = customerId,
                    OfferingId = offeringId,
                    JoinDate = DateTime.UtcNow,
                    IsActive = true
                };

                _db.StudentCourseEnrollments.Add(enrollment);
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

        public async Task<ApiResponse<List<StudentEnrollmentDto>>>
         GetEnrollmentsByOfferingAsync(
             int offeringId,
             Guid branchId,
             Guid tenantId)
        {
            try
            {
                var students = await _db.StudentCourseEnrollments
                    .Where(e =>
                        e.OfferingId == offeringId &&
                        e.Offering.TenantId == tenantId &&
                        e.Offering.BranchId == branchId &&
                        e.IsActive)
                    .Include(e => e.Customer)
                    .Include(e => e.Offering)
                        .ThenInclude(o => o.TrainerCourse)
                            .ThenInclude(tc => tc.Product)
                    .Select(e => new StudentEnrollmentDto
                    {
                        EnrollmentId = e.EnrollmentId,
                        StudentName = e.Customer.CustomerName,
                        CourseName = e.Offering.TrainerCourse.Product.ProductName,
                        JoinDate = e.JoinDate
                    })
                    .ToListAsync();

                return ApiResponseFactory.Success(students);
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<StudentEnrollmentDto>>(
                    "Error fetching enrollments",
                    new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<string>>
        RemoveEnrollmentAsync(
            int enrollmentId,
            Guid branchId,
            Guid tenantId)
        {
            var enrollment = await _db.StudentCourseEnrollments
                .Include(e => e.Offering)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == enrollmentId &&
                    e.Offering.TenantId == tenantId &&
                    e.Offering.BranchId == branchId);

            if (enrollment == null)
                return ApiResponseFactory.Failure<string>(
                    "Enrollment not found for this branch");

            enrollment.IsActive = false;
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success(
                "Enrollment removed successfully");
        }

    }
}
