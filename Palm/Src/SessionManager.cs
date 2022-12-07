using Palm.Cash;
using Palm.Models.Sessions;
using Palm.Models.Sessions.Dto;
using Palm.Models.Users;

namespace Palm;

public class SessionManager
{
    private readonly ISessionСaching _cache;
    private readonly IQuestionsCaching _questionsCache;

    public SessionManager(ISessionСaching cache, IQuestionsCaching questionsCache)
    {
        _cache = cache;
        _questionsCache = questionsCache;
    }

    public void AddSession(Session session)
    {
        session.Id = Guid.NewGuid();
        session.ShortId = session.Id
            .ToString()[..6];
        session.Students = new();
        session.Questions = new();

        _questionsCache.CreateQuestion(session.ShortId);
        _cache.AddSession(session);
    }

    public void AddStudentToSession(Session session, User user)
    {
        // TODO: Проверка если студент уже есть в сессии
        if (_cache.IsExistStudentInSession(session.ShortId, user.Id))
            throw new ArgumentException("Студент уже есть в сессии", nameof(user));

        session.Students.Add(user.Id);
        
        _cache.AddSession(session);
    }

    public void UpdateSession(Session sessionToUpdate)
    {
        Session oldSession = GetSession(sessionToUpdate.ShortId);
        
        oldSession.Questions = sessionToUpdate.Questions;
        
        // TODO: Добавить проверку существования студентов
        oldSession.Students = sessionToUpdate.Students;
        if (!string.IsNullOrEmpty(sessionToUpdate.Title)) 
            oldSession.Title = sessionToUpdate.Title;

        _cache.AddSession(oldSession);
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

    public bool CheckValid(SessionDto session)
    {
        return !(string.IsNullOrEmpty(session.Title) || 
                 session.EndDate == DateTime.MinValue ||
                 session.StartDate == DateTime.MinValue ||
                 session.EndDate > session.StartDate);
    }
}