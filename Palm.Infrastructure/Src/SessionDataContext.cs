using Microsoft.EntityFrameworkCore;
using Palm.Models;
using Palm.Models.Sessions;

namespace Palm.Infrastructure;

public class SessionDataContext : DbContext
{
    public SessionDataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ClassStudents> Classes { get; set; }
    public DbSet<Session> Sessions { get; set; }

    public DbSet<Question> Questions { get; set; }
}