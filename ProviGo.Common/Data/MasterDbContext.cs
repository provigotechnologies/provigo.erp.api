using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;

namespace ProviGo.Common.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }
        public DbSet<Tenant> Tenant => Set<Tenant>();
    }
}
