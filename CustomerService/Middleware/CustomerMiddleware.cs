using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProviGo.Common.Models;
using CustomerService.Services;

namespace CustomerService.Middleware;

public class CustomerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public CustomerMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task Invoke(
        HttpContext context,
        CustomerProvider provider,
        MasterDbContext masterDb)
    {
        // 🔹 Skip swagger
        var path = context.Request.Path.Value?.ToLower();
        if (path!.Contains("/swagger"))
        {
            await _next(context);
            return;
        }

        // 🔹 1️⃣ Get TenantId from HEADER
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid or Missing X-Tenant-Id header");
            return;
        }

        var cacheKey = $"TENANT_{tenantId}";

        // 🔹 2️⃣ Check cache
        if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
        {
            tenant = await masterDb.Tenant
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.TenantId == tenantId && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Tenant not found or inactive");
                return;
            }

            // 🔹 3️⃣ Store in cache
            _cache.Set(cacheKey, tenant, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High
            });
        }

        // 🔹 4️⃣ Set tenant to scoped provider
        provider.SetTenant(tenant);

        await _next(context);
    }
}