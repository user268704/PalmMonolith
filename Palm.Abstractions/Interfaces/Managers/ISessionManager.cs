using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Abstractions.Interfaces.Managers;

public interface ISessionManager
{
    void AddSession(Session session);
    void AddStudentToSession(Session session, User user);
    Session? GetSessionByStudentConnectionId(string connectionId);
    bool IsOwner(User user, string sessionId);
    void UpdateSession(Session updates);
    List<Session> GetAllSessions();
    void RemoveSession(string shortId);
    Session? GetSession(string shortId);
    void EndSession(string sessionId);
}