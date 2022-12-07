using Microsoft.AspNetCore.Identity;

namespace Palm.Models.Users;

public class UserRegister
{
    public string? UserName { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}