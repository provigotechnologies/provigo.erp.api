using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
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
                IShiftService shiftService) =>
            {
                var response = await shiftService.GetShiftsAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/shifts", async (
                ShiftCreateDto dto,
                IShiftService shiftService) =>
            {
                var response = await shiftService.CreateShiftAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/shifts/{id:int}", async (
                int id,
                ShiftUpdateDto dto,
                IShiftService shiftService) =>
            {
                var response = await shiftService.UpdateShiftAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/shifts/{id:int}", async (
                int id,
                IShiftService tenantService) =>
            {
                var response = await tenantService.RemoveShiftAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

        }

    }
}