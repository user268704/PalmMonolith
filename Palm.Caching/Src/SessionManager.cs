using Microsoft.EntityFrameworkCore;
using Palm.Abstractions.Interfaces.Caching;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Abstractions.Interfaces.Repositories;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Caching;

public class SessionManager : ISessionManager
{
    private readonly ISessionСaching _cache;
    private readonly IQuestionsCaching _questionsCache;
    private readonly ISessionRepository _sessionRepository;

    public SessionManager(ISessionСaching cache, ISessionRepository sessionRepository, IQuestionsCaching questionsCache)
    {
        _cache = cache;
        _sessionRepository = sessionRepository;
        _questionsCache = questionsCache;
    }

    public void AddSession(Session session)
    {
        session.Id = Guid.NewGuid();
        session.ShortId = session.Id
            .ToString()[..6];
        session.Students = new List<string>();
        session.Questions = new List<string>();
        session.Takes = new List<Take>();
        session.GroupInfo = new SessionGroupInfo();

        _questionsCache.CreateQuestion(session.ShortId);
        _cache.Create(session);
    }

    public void AddStudentToSession(Session session, User user)
    {
        if (_cache.IsExistStudentInSession(session.ShortId, user.Id))
            throw new ArgumentException("Студент уже есть в сессии", nameof(user));

        session.Students.Add(user.Id);

        _cache.Create(session);
    }

    public Session? GetSessionByStudentConnectionId(string connectionId)
    {
        var sessions = GetAllSessions();

        return sessions.FirstOrDefault(session =>
            session.Takes.FirstOrDefault(take => take.ConnectionId == connectionId) != default);
    }

    public bool IsOwner(User user, string sessionId)
    {
        return GetSession(sessionId)?.HostId.ToString() == user.Id;
    }

    public void UpdateSession(Session updates)
    {
        var oldSession = GetSession(updates.ShortId);

        if (updates.Questions != null) oldSession.Questions.AddRange(updates.Questions);
        if (updates.GroupInfo != null) oldSession.GroupInfo = updates.GroupInfo;
        if (updates.Takes != null)
            oldSession.Takes.AddRange(updates.Takes.Except(oldSession.Takes, new TakeComparer()));
        if (updates.Students != null) oldSession.Students.AddRange(updates?.Students.Except(oldSession.Students));
        if (!string.IsNullOrEmpty(updates.Title)) oldSession.Title = updates.Title;

        _cache.Create(oldSession);
    }

    public List<Session> GetAllSessions()
    {
        return _cache.GetAllSessions();
    }

    public void RemoveSession(string shortId)
    {
        _cache.Delete(shortId);
    }

    public Session? GetSession(string shortId)
    {
        return _cache.Read(shortId);
    }

    public void EndSession(string sessionId)
    {
        var session = GetSession(sessionId);

        try
        {
            _sessionRepository.SaveSession(session);
        }
        catch (DbUpdateException e)
        {
            throw new ArgumentException("Ошибка при сохранении сессии", e);
        }

        _cache.Delete(sessionId);
        _cache.Delete(sessionId + "-questions");
    }
}