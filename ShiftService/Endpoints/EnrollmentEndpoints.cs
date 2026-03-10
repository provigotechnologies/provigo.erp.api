using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ShiftService.DTOs;
using ShiftService.Services;
using ShiftService.Services.Interface;

namespace ShiftService.Endpoints
{
    public class EnrollmentEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Enroll Student
            // ===========================

            app.MapPost("/api/enrollments", async (
                int offeringId,
                int customerId,
                Guid branchId,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .EnrollStudentAsync(offeringId, customerId, branchId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Get Students By Batch

            app.MapGet("/api/enrollments/{offeringId:int}", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .GetEnrollmentsByOfferingAsync(request, includeInactive);

                return Results.Ok(response);
            });

            // 🔹 Remove Enrollment

            app.MapDelete("/api/enrollments/{enrollmentId:int}", async (
                int enrollmentId,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .RemoveEnrollmentAsync(enrollmentId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}