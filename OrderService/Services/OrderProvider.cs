using ProviGo.Common.Models;

namespace OrderService.Services;
public interface IOrderProvider
{
    Guid TenantId { get; }
    string ConnectionString { get; }
}

public class OrderProvider : IOrderProvider
{
    public Guid TenantId { get; private set; }
    public string ConnectionString { get; private set; }

    public void Set(Guid tenantId, int scopeId, string conn)
    {
        TenantId = tenantId;
        ConnectionString = conn;
    }

    public void SetTenant(Tenant tenant)
    {
        TenantId = tenant.TenantId;
        //ScopeId = scopeId;
        ConnectionString = tenant.ConnectionString;
    }
}
