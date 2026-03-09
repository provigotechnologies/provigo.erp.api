using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProviGo.Common.Providers
{
    public interface ITenantProvider
    {
        Guid TenantId { get; }
        string ConnectionString { get; }
    }

    public class TenantProvider : ITenantProvider
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
}