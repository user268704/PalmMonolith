using Palm.Models.Users;

namespace Palm.Models;

public class ClassStudents
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string Name { get; set; }
    public List<User> Students { get; set; }
}