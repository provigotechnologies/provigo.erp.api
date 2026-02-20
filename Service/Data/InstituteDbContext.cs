using Microsoft.EntityFrameworkCore;
using InstituteService.Models;
using System.Collections.Generic;

namespace InstituteService.Data
{
    public class InstituteDbContext : DbContext  // ✅ Must inherit from DbContext
    {
        public InstituteDbContext(DbContextOptions<InstituteDbContext> options)
            : base(options)
        {
        }

        public DbSet<Institute> Institutes { get; set; }  // DbSet for your table
    }
}
