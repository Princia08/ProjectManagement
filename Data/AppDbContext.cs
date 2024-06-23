using Microsoft.EntityFrameworkCore;
using ReportMeeting.Models;

namespace ReportMeeting.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Models.Task> Task { get; set; }
        public DbSet<Platform> Platform { get; set; }
    }
}
