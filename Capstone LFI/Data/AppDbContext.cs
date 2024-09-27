using Microsoft.EntityFrameworkCore;
using Capstone_LFI.Models;

namespace Capstone_LFI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Attendant> Attendant { get; set; }
        public DbSet<AttendantLogs> AttendantLogs { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Receipt> Receipt { get; set; }
        public DbSet<UserLevel> UserLevel { get; set; }
    }
}


