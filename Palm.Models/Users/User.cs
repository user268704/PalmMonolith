using Microsoft.AspNetCore.Identity;

namespace Palm.Models.Users;

public class User : IdentityUser
{
    public string Name { get; set; }
    public string? LastName { get; set; }
}