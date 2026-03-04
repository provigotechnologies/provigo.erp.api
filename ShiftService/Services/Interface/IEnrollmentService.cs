using ProviGo.Common.Models;
using ProviGo.Common.Response;
using ShiftService.DTOs;

namespace ShiftService.Services.Interface
{
    public interface IEnrollmentService
    {
        Task<ApiResponse<string>> EnrollStudentAsync(
            int offeringId,
            int customerId,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<List<StudentEnrollmentDto>>>
            GetEnrollmentsByOfferingAsync(
                int offeringId,
                Guid branchId,
                Guid tenantId);

        Task<ApiResponse<string>> RemoveEnrollmentAsync(
            int enrollmentId,
            Guid branchId,
            Guid tenantId);
    }
}
