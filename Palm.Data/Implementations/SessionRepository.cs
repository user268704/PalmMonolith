using Palm.Abstractions.Interfaces.Data;
using Palm.Infrastructure;
using Palm.Models.Sessions;

namespace Palm.Data.Implementations;

public class SessionRepository : ISessionRepository
{
    private readonly SessionDataContext _context;

    public SessionRepository(SessionDataContext context)
    {
        _context = context;
    }

    public Session GetSession(Guid sessionId)
    {
        Session? session = _context.Sessions.Find(sessionId);
        if (session != null)
            return session;

        throw new Exception("Session not found");
    }

    public void DeleteSession(Guid sessionId)
    {
        Session? session = _context.Sessions.Find(sessionId);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            _context.SaveChanges();
        }


        throw new Exception("Session not found");
        
    }

    public List<Question> GetQuestions(Guid sessionId)
    {
        Session? session = _context.Sessions.Find(sessionId);
        if (session != null)
        {
            List<Question?> result = new();
            foreach (string questionId in session.Questions)
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