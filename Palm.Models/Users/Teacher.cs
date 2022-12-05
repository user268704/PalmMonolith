using Palm.Models.Sessions;

namespace Palm.Models.Users;

public class Teacher
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
    public List<Session> ActiveSessions { get; set; }
    public List<Session> InactiveSessions { get; set; }
}