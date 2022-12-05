using Palm.Models.Sessions;

namespace Palm.Models.Users;

public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public List<Session> HistorySessions { get; set; }
}