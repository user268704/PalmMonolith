using Microsoft.EntityFrameworkCore;
using Palm.Models.Users;

namespace Palm.Infrastructure;

public class UserDataContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Admin> Admins { get; set; }

    public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().Navigation(c => c.HistorySessions).AutoInclude();
    }
}