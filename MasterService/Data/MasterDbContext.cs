using MasterService.Models;
using Microsoft.EntityFrameworkCore;

namespace MasterService.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options)
            : base(options) { }

        public DbSet<ProductMaster> ProductMaster => Set<ProductMaster>();
        public DbSet<GroupMaster> GroupMaster => Set<GroupMaster>();
        public DbSet<BrandMaster> BrandMaster => Set<BrandMaster>();
        public DbSet<UnitMaster> UnitMaster => Set<UnitMaster>();
        public DbSet<HolidayMaster> HolidayMaster => Set<HolidayMaster>();
        public DbSet<DepartmentMaster> DepartmentMaster => Set<DepartmentMaster>();
        public DbSet<DesignationMaster> DesignationMaster => Set<DesignationMaster>();
        public DbSet<ExpenseMaster> ExpenseMaster => Set<ExpenseMaster>();
        public DbSet<KitchenMaster> KitchenMaster => Set<KitchenMaster>();
        public DbSet<TableMaster> TableMaster => Set<TableMaster>();
        public DbSet<ReceipeMaster> ReceipeMaster => Set<ReceipeMaster>();
        public DbSet<LocationMaster> LocationMaster => Set<LocationMaster>();
        public DbSet<UnitSetting> UnitSetting => Set<UnitSetting>();
        public DbSet<PrintParam> PrintParam => Set<PrintParam>();
        public DbSet<KotPrinter> KotPrinter => Set<KotPrinter>();
        public DbSet<KotPrinterSize> KotPrinterSize => Set<KotPrinterSize>();
        public DbSet<ProductType> ProductType => Set<ProductType>();
        public DbSet<TaxSetting> TaxSetting => Set<TaxSetting>();

    }
}
