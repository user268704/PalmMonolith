using Palm.Models.Sessions;

namespace Palm.Abstractions.Interfaces.Repositories;

public interface ISessionRepository
{
    public Session GetSession(Guid sessionId);
    public void SaveSession(Session session);
    public void DeleteSession(Guid sessionId);
    public List<Question> GetQuestions(Guid sessionId);
}