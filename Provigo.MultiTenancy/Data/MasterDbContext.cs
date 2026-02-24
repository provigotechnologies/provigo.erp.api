using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;


namespace ProviGo.MultiTenancy.Data;
public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }
    public DbSet<Tenant> Tenant => Set<Tenant>();
}
