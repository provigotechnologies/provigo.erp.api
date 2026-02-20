using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;

namespace InstituteService.Data
{
    public class InstituteDbContext : DbContext
    {
        public InstituteDbContext(DbContextOptions<InstituteDbContext> options)
            : base(options)
        {
        }

        // ===== Core =====
        public DbSet<TenantDetails> Institutes { get; set; }

        // ===== Users / Identity =====
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UsersLog> UsersLogs { get; set; }

        // ===== Branch & Masters =====
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Tax> Taxes { get; set; }

        // ===== Customers & Attendance =====
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerBatchMapping> CustomerBatchMappings { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        // ===== Orders =====
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderCharge> OrderCharges { get; set; }
        public DbSet<OrderDiscount> OrderDiscounts { get; set; }
        public DbSet<OrderTax> OrderTaxes { get; set; }

        // ===== Payments =====
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Refund> Refunds { get; set; }

        // ===== Subscription & License =====
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<License> Licenses { get; set; }

        // ===== Shifts =====
        public DbSet<Shift> Shifts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 👇 Foreign key for Logs
            modelBuilder.Entity<UsersLog>()
                .HasOne(log => log.User)
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Seed roles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, RoleName = "Admin" },
                new UserRole { Id = 2, RoleName = "User" }
            );

            // ===== Defaults =====
            modelBuilder.Entity<Customer>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ===== UNIQUE Constraints =====

            modelBuilder.Entity<TenantDetails>()
                .HasIndex(i => i.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.TenantId, c.Email })
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.ProductName })
                .IsUnique();

            modelBuilder.Entity<Shift>()
                .HasIndex(s => new { s.TenantId, s.ShiftName })
                .IsUnique();

            modelBuilder.Entity<License>()
                .HasIndex(l => l.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
