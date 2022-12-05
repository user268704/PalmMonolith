using Palm.Models.Cache;
using Palm.Models.Sessions;

namespace Palm.Cash;

public interface IСaching
{
    public void AddSession(Session cache);
    public Session GetSession(string id);
    public List<Session> GetAllSessions();
    public void Remove(string id);
    public void Update(Session oldSession, Session newSession);
    public void Update(Session sessionUpdate);
    public void Clear();
}