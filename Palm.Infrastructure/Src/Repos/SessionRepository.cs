using Palm.Abstractions.Interfaces.Repositories;
using Palm.Models.Sessions;

namespace Palm.Infrastructure.Repos;

public class SessionRepository : ISessionRepository
{
    private readonly SessionDataContext _context;

    public SessionRepository(SessionDataContext context)
    {
        _context = context;
    }

    public Session GetSession(Guid sessionId)
    {
        var session = _context.Sessions.Find(sessionId);
        if (session != null)
            return session;

        throw new Exception("Session not found");
    }

    public void DeleteSession(Guid sessionId)
    {
        var session = _context.Sessions.Find(sessionId);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            _context.SaveChanges();
        }


        throw new Exception("Session not found");
    }

    public List<Question> GetQuestions(Guid sessionId)
    {
        var session = _context.Sessions.Find(sessionId);
        if (session != null)
        {
            List<Question?> result = new();
            foreach (var questionId in session.Questions)
                result.Add(_context.Questions.Find(questionId));

            result.RemoveAll(match => match == null);

            return result;
        }

        throw new Exception("Session not found");
    }

    public void SaveSession(Session session)
    {
        if (session != null)
        {
            _context.Sessions.Add(session);
            _context.SaveChanges();
            return;
        }

        throw new ArgumentNullException(nameof(session));
    }
}