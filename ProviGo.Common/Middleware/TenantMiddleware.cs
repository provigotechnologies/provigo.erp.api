using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProviGo.Common.Models;
using ProviGo.Common.Providers;
using ProviGo.Common.Data;
using System;
using System.Threading.Tasks;

namespace ProviGo.Common.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context, TenantProvider provider, MasterDbContext master)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip certain paths
            if (path!.Contains("/swagger"))
            {
                await _next(context);
                return;
            }            

            var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            if (!Guid.TryParse(tenantHeader, out var tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid or missing X-Tenant-Id header");
                return;
            }

            var cacheKey = $"TENANT_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                tenant = await master.Tenant
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsActive);

                if (tenant == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Tenant not found or inactive");
                    return;
                }

                _cache.Set(cacheKey, tenant, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            provider.SetTenant(tenant);

            await _next(context);
        }
    }
}