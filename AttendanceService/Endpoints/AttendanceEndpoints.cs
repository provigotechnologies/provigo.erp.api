using AttendanceService.DTOs;
using AttendanceService.Services;
using AttendanceService.Services.Interface;
using ProviGo.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;

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
                TenantProvider tenantProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.GetAttendanceRecordsAsync(request, includeInactive, tenantProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/attendance", async (
                AttendanceCreateDto dto,
                 TenantProvider tenantProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.CreateAttendanceRecordAsync(dto, tenantProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/products/{id:int}", async (
                int id,
                AttendanceUpdateDto dto,
                TenantProvider tenantProvider,
                 [FromServices] IAttendanceService AttendanceService) =>
            {
                var response = await AttendanceService.UpdateAttendanceRecordAsync(id, dto, tenantProvider.TenantId);
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
