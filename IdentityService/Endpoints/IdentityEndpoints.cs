using IdentityService.DTOs;
using IdentityService.Services;
using IdentityService.Utils;
using Microsoft.AspNetCore.Http;
using ProviGo.Common.Pagination;
using ProviGo.Common.Services;
public static class IdentityEndpoints
{
    public static void Map(WebApplication app)
    {
        // ---------------- REGISTER ----------------
        app.MapPost("/api/register", async (UserCreateRequest dto, IIdentityService service) =>
        {
            // BranchIds must come from DTO for role assignment
            var result = await service.RegisterAsync(dto, dto.BranchIds ?? new List<Guid>());
            return result.Success 
            ? Results.Created("/api/users", result) 
            : Results.BadRequest(result);
        });

        // ---------------- LOGIN ----------------
        app.MapPost("/api/login", async (LoginDto dto, IIdentityService service) =>
        {
            var result = await service.LoginAsync(dto);
            return result.Success
                ? Results.Ok(result)
                : Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        });

        // ---------------- GET USERS ----------------
        app.MapGet("/api/users", async (HttpContext ctx,
         [AsParameters] PaginationRequest request,
         bool includeInactive,
         IIdentityService service) =>
        {
            var result = await service.GetUsersAsync(request, includeInactive);
            return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
        }).RequireAuthorization();

        // ---------------- UPDATE USER ----------------
        app.MapPut("/api/users/{id:guid}", async (
         Guid id,
         UserUpdateRequest dto,
         HttpContext ctx,
         IIdentityService service) =>
        {
            var result = await service.UpdateUserAsync(id, dto);
            return result.Success 
            ? Results.Ok(result)
            : Results.NotFound(result);
        }).RequireAuthorization();

        // ---------------- DELETE USER ----------------
        app.MapDelete("/api/users/{id:guid}", async (
         Guid id,
         IIdentityService service,
         CurrentUserService currentUser) =>
        {
            var currentUserId = currentUser.UserId;

            var result = await service.DeleteUserAsync(id);

            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        }).RequireAuthorization();

        // ---------------- GET ROLES ----------------
        app.MapGet("/api/roles", async (IIdentityService service) =>
        {
            return Results.Ok(await service.GetRolesAsync());
        }).RequireAuthorization();

        // ---------------- GET LOGS ----------------
        app.MapGet("/api/logs", async (IIdentityService service) =>
        {
            return Results.Ok(await service.GetLogsAsync());
        }).RequireAuthorization();

        // ---------------- LOGOUT ----------------
        app.MapPost("/api/logout", async (
         CurrentUserService currentUser,
         IIdentityService service) =>
        {
            var result = await service.LogoutAsync(
                new LogoutRequest { UserId = currentUser.UserId });

            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }).RequireAuthorization();

    }
}