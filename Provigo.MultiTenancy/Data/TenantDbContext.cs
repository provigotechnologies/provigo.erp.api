using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;
using ProviGo.MultiTenancy;

namespace ProviGo.MultiTenancy.Data;
public class TenantDbContext : DbContext
{

    private readonly ITenantProvider? _tenant;

    // Runtime constructor
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantProvider tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // Design-time constructor 
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options)
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

        modelBuilder.Entity<UsersLog>()
        .HasOne(l => l.User)
        .WithMany(u => u.UsersLogs)
        .HasForeignKey(l => l.UserId)
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
            .HasOne(s => s.User)
            .WithMany(u => u.Shifts)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<License>()
            .HasIndex(l => l.LicenseKey)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }

    // ===== Core =====
    public DbSet<TenantDetails> TenantDetails { get; set; }

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
    public DbSet<TrainerShiftMapping> CustomerBatchMappings { get; set; }
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

}
