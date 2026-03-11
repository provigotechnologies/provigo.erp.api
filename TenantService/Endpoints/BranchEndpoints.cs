using ProviGo.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using TenantService.DTOs;
using TenantService.Services.Interface;
using TenantService.Services;
using ProviGo.Common.Providers;

namespace TenantService.Endpoints
{
    public static class BranchEndpoints
    {
        public static void Map(WebApplication app)
        {
         
            // ===========================
            // 🔹 BRANCHES
            // ===========================

            app.MapGet("/api/branches", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                IBranchService branchService) =>
            {
                var response = await branchService.GetBranchesAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/branches", async (
                BranchCreateDto dto,
                IBranchService branchService) =>
            {
                var response = await branchService.CreateBranchAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/branches/{branchId:guid}", async (
                Guid branchId,
                BranchUpdateDto dto,
                IBranchService branchService) =>
            {
                var response = await branchService.UpdateBranchAsync(branchId, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/branches/{branchId:guid}", async (
                Guid branchId,
                IBranchService branchService) =>
            {
                var response = await branchService.RemoveBranchAsync(branchId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapGet("/api/states-dropdown", async (
            IBranchService branchService) =>
            {
                var data = await branchService.GetStateDropdownAsync();
                return Results.Ok(data);
            });

        }

    }
}
