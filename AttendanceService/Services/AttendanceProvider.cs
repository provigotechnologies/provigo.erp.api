using ProviGo.Common.Models;

namespace AttendanceService.Services
{
    public interface IAttendanceProvider
    {
        Guid TenantId { get; }
        string ConnectionString { get; }
        void SetTenant(Tenant tenant);
    }

    public class AttendanceProvider : IAttendanceProvider
    {
        public Guid TenantId { get; private set; }
        public string ConnectionString { get; private set; }

        public void SetTenant(Tenant tenant)
        {
            TenantId = tenant.TenantId;
            ConnectionString = tenant.ConnectionString;
        }
    }
}
