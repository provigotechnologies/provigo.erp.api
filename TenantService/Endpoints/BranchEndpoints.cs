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
                [FromServices] TenantProvider tenantProvider,
                IBranchService branchService) =>
            {
                var response = await branchService.GetBranchesAsync(request, includeInactive, tenantProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/branches", async (
                BranchCreateDto dto,
                [FromServices] TenantProvider tenantProvider,
                IBranchService branchService) =>
            {
                var response = await branchService.CreateBranchAsync(dto, tenantProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/branches/{branchId:guid}", async (
                Guid branchId,
                BranchUpdateDto dto,
                [FromServices] TenantProvider tenantProvider,
                IBranchService branchService) =>
            {
                var response = await branchService.UpdateBranchAsync(branchId, dto, tenantProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/branches/{branchId:guid}", async (
                Guid branchId,
                [FromServices] TenantProvider tenantProvider,
                IBranchService branchService) =>
            {
                var response = await branchService.RemoveBranchAsync(branchId, tenantProvider.TenantId);
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
