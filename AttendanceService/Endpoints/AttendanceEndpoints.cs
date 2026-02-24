using AttendanceService.DTOs;
using AttendanceService.Services;
using AttendanceService.Services.Interface;
using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using ProviGo.Common.Pagination;

namespace AttendanceService.Endpoints
{
    public class AttendanceEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Products
            // ===========================

            app.MapGet("/api/attendance", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                AttendanceProvider attendanceProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.GetAttendanceRecordsAsync(request, includeInactive, attendanceProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/attendance", async (
                AttendanceCreateDto dto,
                 AttendanceProvider attendanceProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.CreateAttendanceRecordAsync(dto, attendanceProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/products/{id:int}", async (
                int id,
                AttendanceUpdateDto dto,
                AttendanceProvider attendanceProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.UpdateAttendanceRecordAsync(id, dto, attendanceProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            /*app.MapDelete("/api/products/{id:int}", async (
                int id,
                [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.RemoveAttendanceRecordAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });*/

        }

    }
}
