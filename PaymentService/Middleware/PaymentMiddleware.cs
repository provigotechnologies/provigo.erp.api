using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProviGo.Common.Models;
using IdentityService.Data;
using PaymentService.Services;
using PaymentService.Services.Extensions;
using IdentityService.Services;

namespace PaymentService.Middleware
{
    public class PaymentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public PaymentMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(
            HttpContext context,
            MasterDbContext masterDb,
            IdentityProvider provider)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip swagger
            if (path != null && path.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            var tenantIdHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            if (!Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Invalid Tenant Id"
                });
                return;
            }

            var cacheKey = $"TENANT_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                tenant = await masterDb.Tenant
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.TenantId == tenantId && t.IsActive);

                if (tenant == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Tenant not found"
                    });
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

            // 🔥 Set tenant globally for this request
            provider.SetTenant(tenant);

            await _next(context);
        }
    }

}
