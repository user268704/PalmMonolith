using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Palm.Models.Users;

namespace Palm.Infrastructure;

public class UserDataContext : IdentityDbContext<User>
{
    public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
    {
        
    }
}