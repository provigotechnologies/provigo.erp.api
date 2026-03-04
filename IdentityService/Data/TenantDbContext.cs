using IdentityService.Services;
using InvoiceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;

namespace IdentityService.Data;
public class TenantDbContext : DbContext
{

    private readonly IdentityProvider? _tenant;

    // Runtime constructor
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        IdentityProvider tenant)
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

        modelBuilder.Entity<License>()
            .HasIndex(l => l.LicenseKey)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<State>()
            .HasOne(s => s.Country)
            .WithMany()
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Global Decimal Precision Fix =====
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var decimalProperties = entity.ClrType
                .GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) ||
                            p.PropertyType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                modelBuilder.Entity(entity.Name)
                    .Property(property.Name)
                    .HasPrecision(18, 2);
            }
        }
    }

    // ===== Core =====
    public DbSet<TenantDetails> TenantDetails { get; set; }

    // ===== Masters =====
    public DbSet<Country> Countries { get; set; }
    public DbSet<State> States { get; set; }

    // ===== Users / Identity =====
    public DbSet<User> Users { get; set; }
    public DbSet<UserBranch> UserBranches { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UsersLog> UsersLogs { get; set; }

    // ===== Branch & Masters =====
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Charge> Charges { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<TrainerCourse> TrainerCourses { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }

    public DbSet<Tax> Taxes { get; set; }

    // ===== Customers & Attendance =====
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CourseOffering> CustomerBatchMappings { get; set; }
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
    public DbSet<StudentCourseEnrollment> StudentCourseEnrollments { get; set; }


    // ===== Invoice =====
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }


}
