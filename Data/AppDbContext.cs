using Microsoft.EntityFrameworkCore;
using ReportMeeting.Models;

namespace ReportMeeting.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Users> users { get; set; }
        public DbSet<Role> role { get; set; }
    }
}
