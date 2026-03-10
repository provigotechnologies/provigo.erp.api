using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ShiftService.DTOs;

namespace ShiftService.Services.Interface
{
    public interface IEnrollmentService
    {
        Task<ApiResponse<string>> EnrollStudentAsync(
            int offeringId,
            int customerId,
            Guid branchId);

        Task<ApiResponse<List<CustomerProductEnrollment>>> GetEnrollmentsByOfferingAsync(
            PaginationRequest request,
            bool includeInactive);

        Task<ApiResponse<string>> RemoveEnrollmentAsync(int enrollmentId);
    }
}