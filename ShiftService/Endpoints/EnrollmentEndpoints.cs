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
                ShiftProvider shiftProvider,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .EnrollStudentAsync(offeringId, customerId, branchId, shiftProvider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Get Students By Batch

            app.MapGet("/api/enrollments/{offeringId:int}", async (
                int offeringId,
                Guid branchId,
                ShiftProvider shiftProvider,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .GetEnrollmentsByOfferingAsync(offeringId, branchId, shiftProvider.TenantId);

                return Results.Ok(response);
            });

            // 🔹 Remove Enrollment

            app.MapDelete("/api/enrollments/{enrollmentId:int}", async (
                int enrollmentId,
                Guid branchId,
                ShiftProvider shiftProvider,
                IEnrollmentService enrollmentService) =>
            {
                var response = await enrollmentService
                    .RemoveEnrollmentAsync(enrollmentId, branchId, shiftProvider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}