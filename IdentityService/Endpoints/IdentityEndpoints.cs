using IdentityService.DTOs;
using IdentityService.Services;
using IdentityService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using System.Security.Claims;

public static class IdentityEndpoints
{
    public static void Map(WebApplication app)
    {
        User user = new();

        // User Register
        app.MapPost("/api/register", async (
            UserCreateRequest dto, 
            IIdentityService service,
            Guid branchId,
            IdentityProvider tenantProvider) =>
        {
            var result = await service.RegisterAsync(dto, branchId, tenantProvider.TenantId);

            return result.Success
                ? Results.Created("/api/users", result)
                : Results.BadRequest(result);
        });

        // Get Roles
        app.MapGet("/api/roles", async (IIdentityService service) =>
        {
            return Results.Ok(await service.GetRolesAsync());
        });

        // Get Logs
        app.MapGet("/api/logs", async (IIdentityService service) =>
        {
            return Results.Ok(await service.GetLogsAsync());
        }).RequireAuthorization();

        // Log Logout
        app.MapPost("/api/logout", async (LogoutRequest dto, IIdentityService service) =>
        {
            var result = await service.LogoutAsync(dto);

            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }).RequireAuthorization();

        // User Login
        app.MapPost("/api/login", async (LoginDto dto, IIdentityService service) =>
        {
            var result = await service.LoginAsync(dto);

            return result.Success
                ? Results.Ok(result)
                : Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        });

        // Get Users
        app.MapGet("/api/users", async (
          [AsParameters] PaginationRequest request,
          bool includeInactive,
          ClaimsPrincipal claims,
          Guid branchId,
          IIdentityService service) =>
        {
            var tenantId = Guid.Parse(claims.FindFirst("tenantId")!.Value);

            var result = await service.GetUsersAsync(request, includeInactive, branchId, tenantId);

            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }).RequireAuthorization();

        // Update Users
        app.MapPut("/api/users/{id:guid}", async (
           Guid id,
           UserUpdateRequest dto,
           ClaimsPrincipal claims,
           Guid branchId,
           IIdentityService service) =>
        {
            var tenantId = Guid.Parse(claims.FindFirst("tenantId")!.Value);

            var result = await service.UpdateUserAsync(id, dto, branchId, tenantId);

            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        }).RequireAuthorization(); 

        // Delete Users
        app.MapDelete("/api/users/{id:guid}", async (
            Guid id,
            ClaimsPrincipal claims,
            Guid branchId,
            IIdentityService service) =>
        {
            var tenantId = Guid.Parse(claims.FindFirst("tenantId")!.Value);

            var result = await service.DeleteUserAsync(id, branchId, tenantId);

            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        }).RequireAuthorization();

    }
}

