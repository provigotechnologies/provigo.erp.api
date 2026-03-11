using Microsoft.AspNetCore.Http;
using ProviGo.Common.Constants;
using System.Security.Claims;

namespace ProviGo.Common.Services
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public Guid UserId =>
            Guid.TryParse(_http.HttpContext?.User?.FindFirst("userId")?.Value, out var id)
                ? id : Guid.Empty;

        public Guid TenantId =>
            Guid.TryParse(_http.HttpContext?.User?.FindFirst("tenantId")?.Value, out var id)
                ? id : Guid.Empty;

        public string Role =>
            _http.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

        public string Email =>
            _http.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "";

        public bool IsAuthenticated =>
            _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsSuperAdmin =>
            Role.Equals(Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

        public bool IsAdmin =>
            Role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase);

        public bool IsUser =>
            Role.Equals(Roles.User, StringComparison.OrdinalIgnoreCase);

        public void EnsureWriteAccess()
        {
            if (IsUser)
                throw new UnauthorizedAccessException("User has read-only access");
        }

        public void EnsureAuthenticated()
        {
            if (!IsAuthenticated)
                throw new UnauthorizedAccessException("User not authenticated");
        }

        public List<Guid> BranchIds =>
        _http.HttpContext?.User?
            .FindAll("branchId")
            .Select(c => Guid.Parse(c.Value))
            .ToList() ?? new List<Guid>();
    }
}