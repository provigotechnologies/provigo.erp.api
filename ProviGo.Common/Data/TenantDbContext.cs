using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;
using ProviGo.Common.Providers;

namespace ProviGo.Common.Data
{
    public class TenantDbContext : DbContext
    {
        private readonly TenantProvider? _tenant;

        // expose tenant id for global filters
        public Guid CurrentTenantId => _tenant?.TenantId ?? Guid.Empty;

        // Runtime constructor
        public TenantDbContext(
            DbContextOptions<TenantDbContext> options,
            TenantProvider tenant)
            : base(options)
        {
            _tenant = tenant;
        }

        // Design time constructor (for migrations)
        public TenantDbContext(DbContextOptions<TenantDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured &&
                _tenant != null &&
                !string.IsNullOrEmpty(_tenant.ConnectionString))
            {
                options.UseMySql(
                    _tenant.ConnectionString,
                    ServerVersion.AutoDetect(_tenant.ConnectionString));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== RELATIONSHIPS =====

            modelBuilder.Entity<UsersLog>()
                .HasOne(l => l.User)
                .WithMany(u => u.UsersLogs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<State>()
                .HasOne(s => s.Country)
                .WithMany()
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== SEED ROLES =====

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, RoleName = "SuperAdmin" },
                new UserRole { Id = 2, RoleName = "Admin" },
                new UserRole { Id = 3, RoleName = "User" }
            );

            // ===== DEFAULT VALUES =====

            modelBuilder.Entity<Customer>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ===== UNIQUE INDEXES =====

            modelBuilder.Entity<TenantDetails>()
                .HasIndex(i => i.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.TenantId, c.BranchId, c.Email })
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.BranchId, p.ProductName })
                .IsUnique();

            modelBuilder.Entity<License>()
                .HasIndex(l => l.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();

            // ===== GLOBAL DECIMAL PRECISION =====

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var decimalProps = entity.ClrType
                    .GetProperties()
                    .Where(p =>
                        p.PropertyType == typeof(decimal) ||
                        p.PropertyType == typeof(decimal?));

                foreach (var prop in decimalProps)
                {
                    modelBuilder.Entity(entity.Name)
                        .Property(prop.Name)
                        .HasPrecision(18, 2);
                }
            }

            // ===== GLOBAL TENANT FILTER =====

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Branch>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<User>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Order>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Payment>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Invoice>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
        }

        // ===== CORE =====

        public DbSet<TenantDetails> TenantDetails { get; set; }

        // ===== MASTER =====

        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }

        // ===== USERS =====

        public DbSet<User> Users { get; set; }
        public DbSet<UserBranch> UserBranches { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UsersLog> UsersLogs { get; set; }

        // ===== BRANCH =====

        public DbSet<Branch> Branches { get; set; }

        // ===== PRODUCT / MASTER =====

        public DbSet<Product> Products { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Tax> Taxes { get; set; }

        public DbSet<TrainerCourse> TrainerCourses { get; set; }
        public DbSet<CourseOffering> CourseOfferings { get; set; }

        // ===== CUSTOMERS =====

        public DbSet<Customer> Customers { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        // ===== ORDERS =====

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderCharge> OrderCharges { get; set; }
        public DbSet<OrderDiscount> OrderDiscounts { get; set; }
        public DbSet<OrderTax> OrderTaxes { get; set; }

        // ===== PAYMENTS =====

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Refund> Refunds { get; set; }

        // ===== SUBSCRIPTION =====

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<License> Licenses { get; set; }

        // ===== SHIFT =====

        public DbSet<Shift> Shifts { get; set; }
        public DbSet<StudentCourseEnrollment> StudentCourseEnrollments { get; set; }

        // ===== INVOICE =====

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
    }
}