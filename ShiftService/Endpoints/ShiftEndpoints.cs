using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using ShiftService.DTOs;
using ShiftService.Services;
using ShiftService.Services.Interface;

namespace ShiftService.Endpoints
{
    public class ShiftEndpoints
    {
        public static void Map(WebApplication app)
        {

            // ===========================
            // 🔹 Shifts
            // ===========================

            app.MapGet("/api/shifts", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                ShiftProvider shiftProvider,
                IShiftService shiftService) =>
            {
                var response = await shiftService.GetShiftsAsync(request, includeInactive, shiftProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/shifts", async (
                ShiftCreateDto dto,
                ShiftProvider shiftProvider,
                IShiftService shiftService) =>
            {
                var response = await shiftService.CreateShiftAsync(dto, shiftProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/shifts/{id:int}", async (
                int id,
                ShiftUpdateDto dto,
                ShiftProvider shiftProvider,
                IShiftService shiftService) =>
            {
                var response = await shiftService.UpdateShiftAsync(id, dto, shiftProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/shifts/{id:int}", async (
                int id,
                ShiftProvider shiftProvider,
                IShiftService tenantService) =>
            {
                var response = await tenantService.RemoveShiftAsync(id, shiftProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

        }

    }
}
