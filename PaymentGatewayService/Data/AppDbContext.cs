using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Models;

namespace PaymentGatewayService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<OrderRecord> Orders { get; set; }
        // public DbSet<RazorpayKey> RazorpayKeys { get; set; } // optional
    }
}
