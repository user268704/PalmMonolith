using Microsoft.AspNetCore.Identity;
using Palm.Models.Sessions;

namespace Palm.Models.Users;

public class User : IdentityUser
{
    public string Name { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public List<Session> HistorySessions { get; set; }
}