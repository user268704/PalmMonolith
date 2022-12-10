using Palm.Models.Sessions;

namespace Palm.Abstractions.Interfaces.Data;

public interface ISessionСaching
{
    public void AddSession(Session cache);
    public Session? GetSession(string id);
    public List<Session> GetAllSessions();
    public bool IsExistStudentInSession(string sessionShortId, string userId);
    public void Remove(string id);
    public void Update(Session oldSession, Session newSession);
    public void Update(Session sessionUpdate);
    public void Clear();
}