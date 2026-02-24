using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using TenantService.DTOs;
using TenantService.Services.Interface;
using TenantService.Services;

namespace TenantService.Endpoints
{
    public static class TenantEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Upload Tenant Logo
            // ===========================
            app.MapPost("/tenants/{id:guid}/logo", async (
                Guid id,
                HttpRequest request,
                TenantDbContext db) =>
            {
                var tenantDetails = await db.TenantDetails
                    .FirstOrDefaultAsync(i => i.TenantId == id);

                if (tenantDetails == null)
                    return Results.NotFound("Tenant not found");

                var file = request.Form.Files["logo"];
                if (file == null || file.Length == 0)
                    return Results.BadRequest("No logo file uploaded");

                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "tenants"
                );

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileExt = Path.GetExtension(file.FileName);
                var fileName = $"tenant_{id}{fileExt}";
                var filePath = Path.Combine(uploadPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var logoUrl = $"/uploads/tenants/{fileName}";
                tenantDetails.LogoUrl = logoUrl;

                await db.SaveChangesAsync();

                return Results.Ok(new { logoUrl });
            });


            // ===========================
            // 🔹 TENANTS
            // ===========================

            app.MapGet("/api/tenants", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                ITenantService tenantService) =>
            {
                var response = await tenantService.GetTenantsAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/tenants", async (
                TenantCreateDto dto,
                ITenantService tenantService) =>
            {
                var response = await tenantService.CreateTenantAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/tenants/{id:guid}", async (
                Guid id,
                TenantUpdateDto dto,
                ITenantService tenantService) =>
            {
                var response = await tenantService.UpdateTenantAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/tenants/{id:guid}", async (
                Guid id,
                ITenantService tenantService) =>
            {
                var response = await tenantService.RemoveTenantAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


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

            /*  app.MapGet("/api/tenants/{tenantId:guid}/branches/{branchId:guid}",
                  async (Guid tenantId,
                         Guid branchId,
                         TenantProvider productProvider,
                         IBranchService branchService) =>
                  {
                      var response = await branchService
                          .GetBranchByIdAsync(branchId, );

                      return response.Success
                          ? Results.Ok(response)
                          : Results.NotFound(response);
                  });*/

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
        }
    }
}
