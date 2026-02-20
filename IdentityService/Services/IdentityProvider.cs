
using ProviGo.Common.Models;

namespace IdentityService.Services;
public interface IIdentityProvider
{
    Guid TenantId { get; }
    string ConnectionString { get; }
}

public class IdentityProvider : IIdentityProvider
{
    public Guid TenantId { get; private set; }
    public string ConnectionString { get; private set; }

    public void Set(Guid tenantId, string conn)
    {
        TenantId = tenantId;
        ConnectionString = conn;
    }

    public void SetTenant(Tenant tenant)
    {
        TenantId = tenant.TenantId;
        ConnectionString = tenant.ConnectionString;
    }
}
