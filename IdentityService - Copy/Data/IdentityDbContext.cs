using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UsersLog> UsersLog => Set<UsersLog>();
        public DbSet<UserRole> UserRole => Set<UserRole>();

        // 👇 Add this method here
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
        }

    }
}
