using Palm.Cash;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionManager
{
    private readonly IСaching _cache;

    public SessionManager(IСaching cache)
    {
        _cache = cache;
    }
    
    public void AddSession(Session session)
    {
        session.Id = Guid.NewGuid();
        session.ShortId = session.Id
            .ToString()[..6];
        session.Students = new();
        session.Questions = new();
        
        _cache.AddSession(session);
    }
    
    public void AddStudentToSession(Session session, Student student)
    {
        // TODO: Проверка если студент уже есть в сессии
        _cache.Remove(session.ShortId);
        session.Students.Add(student);
        _cache.AddSession(session);
    }

    public List<Session> GetAllSessions()
    {
        return _cache.GetAllSessions();
    }

    public void RemoveSession(string shortId)
    {
        _cache.Remove(shortId);
    }
    
    public Session GetSession(string id)
    {
        return _cache.GetSession(id);
    }
}