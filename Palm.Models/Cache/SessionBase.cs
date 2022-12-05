using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Models.Cache;

public abstract class SessionBase : CacheBase
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string Title { get; set; }
    public List<Student> Students { get; set; }
    public List<Question> Questions { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}