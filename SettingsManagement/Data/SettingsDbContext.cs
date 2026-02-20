using Microsoft.EntityFrameworkCore;
using SettingsManagement.Models.Master;
using SettingsManagement.Models.View;

namespace SettingsManagement.Data
{
    public class SettingsDbContext : DbContext
    {
        public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
          : base(options) { }
        public DbSet<SalesSetting> SaleSetting => Set<SalesSetting>();
        public DbSet<PurchaseSetting> PurchaseSetting => Set<PurchaseSetting>();
        public DbSet<InventorySetting> InventorySetting => Set<InventorySetting>();
        public DbSet<StaffSetting> StaffSetting => Set<StaffSetting>();
        public DbSet<AccountSetting> AccountSetting => Set<AccountSetting>();
        public DbSet<ReportSetting> ReportSetting => Set<ReportSetting>();
        public DbSet<MasterSetting> MasterSetting => Set<MasterSetting>();
        public DbSet<MiscSetting> MiscSetting => Set<MiscSetting>();

        public DbSet<SaleAccess> SaleAccess => Set<SaleAccess>();
        public DbSet<PurchaseAccess> PurchaseAccess => Set<PurchaseAccess>();
        public DbSet<InventoryAccess> InventoryAccess => Set<InventoryAccess>();
        public DbSet<StaffAccess> StaffAccess => Set<StaffAccess>();
        public DbSet<AccountAccess> AccountAccess => Set<AccountAccess>();
        public DbSet<ReportAccess> ReportAccess => Set<ReportAccess>();
        public DbSet<MasterAccess> MasterAccess => Set<MasterAccess>();
        public DbSet<MiscAccess> MiscAccess => Set<MiscAccess>();

    }
}
