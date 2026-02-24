using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProviGo.Common.Models;
using IdentityService.Data;
using AttendanceService.Services;
using AttendanceService.Services.Extensions;
using IdentityService.Services;

namespace AttendanceService.Middleware
{
    public class AttendanceMiddleware
    {
        private readonly RequestDelegate _next;

        public AttendanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
                                 IAttendanceProvider provider,
                                 MasterDbContext masterDb)
        {
            var tenantIdHeader =
                context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            if (!Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid Tenant Id");
                return;
            }

            var tenant = await masterDb.Tenant
                .FirstOrDefaultAsync(t =>
                    t.TenantId == tenantId && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant not found");
                return;
            }

            provider.SetTenant(tenant);

            await _next(context);
        }
    }

}
