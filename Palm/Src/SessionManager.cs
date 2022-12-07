using Palm.Cash;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm;

public class SessionManager
{
    private readonly ISessionСaching _cache;

    public SessionManager(ISessionСaching cache)
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
    
    public void AddStudentToSession(Session session, User user)
    {
        // TODO: Проверка если студент уже есть в сессии
        if (_cache.IsExistStudentInSession(session.ShortId, user.Id))
            throw new ArgumentException("Студент уже есть в сессии", nameof(user));
        
        _cache.Remove(session.ShortId);
        
        session.Students.Add(user.Id);
        
        _cache.AddSession(session);
    }

    public void UpdateSession(Session session)
    {
        
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

    private Session AddQuestionsToSession(Session session, List<Question> questions)
    {
        session.Questions.AddRange(questions);
    }
}