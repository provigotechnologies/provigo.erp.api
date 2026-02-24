using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProviGo.Common.Models;

namespace ProviGo.MultiTenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task Invoke(HttpContext context,
                         TenantProvider provider,
                         MasterDbContext master)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Skip swagger
        if (path!.Contains("/swagger"))
        {
            await _next(context);
            return;
        }

        var identityIdHeader = context.Request.Headers["X-Tenant-Id"]
            .FirstOrDefault();

        if (!Guid.TryParse(identityIdHeader, out var tenantId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid Tenant Id");
            return;
        }

        var tokenTenantId = context.User.FindFirst("tenantId")?.Value;

        if (path.Contains("/login") || path.Contains("/register"))
        {
            tokenTenantId = tenantId.ToString();
        }

        if (tokenTenantId == null || tokenTenantId != tenantId.ToString())
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid tenant access");
            return;
        }

        var cacheKey = $"TENANT_{tenantId}";
        if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
        {
            tenant = await master.Tenant
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.TenantId == tenantId && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant not found");
                return;
            }

            _cache.Set(cacheKey, tenant,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(30),
                    SlidingExpiration =
                        TimeSpan.FromMinutes(10)
                });
        }

        provider.SetTenant(tenant);

        await _next(context);
    }
}

