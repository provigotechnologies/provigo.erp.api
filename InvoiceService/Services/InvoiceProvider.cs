using ProviGo.Common.Models;

namespace InvoiceService.Services
{
    public interface IInvoiceProvider
    {
        Guid TenantId { get; }
        string ConnectionString { get; }
    }

    public class InvoiceProvider : IInvoiceProvider
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
}
