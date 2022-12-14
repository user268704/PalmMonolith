using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Abstractions.Interfaces.Managers;

public interface ISessionManager
{
    void CreateSession(Session session);
    void AddStudentToSession(Session session, User user);
    Session? GetSessionByStudentConnectionId(string connectionId);
    Session? GetSessionByTeacherConnectionId(string connectionId);
    Session? GetSessionsByStudent(string studentId);
    bool IsOwner(User user, Session session);
    void AddUpdates(Session updates);
    void UpdateSession(Session session);
    void AddQuestions(List<Question> questions, Session session);
    List<Session> GetAllSessions();
    bool ReplyToQuestion(Session session, User user, string questionId, string answerId);
    void RemoveSession(string shortId);
    Session? GetSession(string shortId);
    void EndSession(string sessionId);
}