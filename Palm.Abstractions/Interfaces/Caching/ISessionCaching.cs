using Palm.Models.Sessions;

namespace Palm.Abstractions.Interfaces.Caching;

public interface ISessionCaching
{
    public void Create(Session cache);
    public Session? Read(string id);
    public List<Session> GetAllSessions();
    public bool IsExistStudentInSession(string sessionShortId, string userId);
    public void Delete(string id);
    public void Update(Session sessionUpdate);
    public void Clear();
}