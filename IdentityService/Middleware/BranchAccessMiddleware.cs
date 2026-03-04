using System.Security.Claims;

namespace IdentityService.Middleware
{
    public class BranchAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public BranchAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "SuperAdmin")
            {
                await _next(context);
                return;
            }

            var branchHeader = context.Request.Headers["X-Branch-Id"]
                .FirstOrDefault();

            if (!Guid.TryParse(branchHeader, out var branchId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid Branch Id");
                return;
            }

            var branchesClaim = context.User.FindFirst("branches")?.Value;

            if (branchesClaim == null ||
                !branchesClaim.Split(',').Contains(branchId.ToString()))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Branch access denied");
                return;
            }

            await _next(context);
        }
    }
}
