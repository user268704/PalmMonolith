using Microsoft.EntityFrameworkCore;
using Palm.Abstractions.Interfaces.Caching;
using Palm.Abstractions.Interfaces.Managers;
using Palm.Abstractions.Interfaces.Repositories;
using Palm.Models.Sessions;
using Palm.Models.Users;

namespace Palm.Caching;

public class SessionManager : ISessionManager
{
    private readonly IQuestionsCaching _questionsCache;
    private readonly ISessionCaching _sessionCaching;
    private readonly ISessionRepository _sessionRepository;

    public SessionManager(ISessionCaching sessionCaching, ISessionRepository sessionRepository, IQuestionsCaching questionsCache)
    {
        _sessionCaching = sessionCaching;
        _sessionRepository = sessionRepository;
        _questionsCache = questionsCache;
    }

    public void CreateSession(Session session)
    {
        session.Id = Guid.NewGuid();
        session.ShortId = session.Id
            .ToString()[..6];
        session.Students = new List<string>();
        session.Questions = new List<string>();
        session.Takes = new List<Take>();
        session.GroupInfo = new SessionGroupInfo();

        _questionsCache.CreateQuestion(session.ShortId);
        _sessionCaching.Create(session);
    }

    public void AddStudentToSession(Session session, User user)
    {
        if (_sessionCaching.IsExistStudentInSession(session.ShortId, user.Id))
            throw new ArgumentException("Студент уже есть в сессии", nameof(user));

        session.Students.Add(user.Id);

        _sessionCaching.Create(session);
    }

    public Session? GetSessionByStudentConnectionId(string connectionId)
    {
        var sessions = GetAllSessions();

        return sessions.FirstOrDefault(session =>
            session.Takes.FirstOrDefault(take => take.ConnectionId == connectionId) != default);
    }

    public Session? GetSessionByTeacherConnectionId(string connectionId)
    {
        var sessions = GetAllSessions();

        return sessions.FirstOrDefault(session => session.GroupInfo.TeacherId == connectionId);
    }

    public Session? GetSessionsByStudent(string studentId)
    {
        var sessions = GetAllSessions();
        
        return sessions.Find(session => session.Students.Any(s => s == studentId));
    }

    public bool IsOwner(User user, Session session) =>
        session.HostId.ToString() == user.Id;

    public void AddUpdates(Session updates)
    {
        var oldSession = GetSession(updates.ShortId);

        if (updates.Questions != null) oldSession.Questions.AddRange(updates.Questions.Except(oldSession.Questions));
        if (updates.GroupInfo != null) oldSession.GroupInfo = updates.GroupInfo;
        if (updates.Takes != null)
            oldSession.Takes.AddRange(updates.Takes.Except(oldSession.Takes, new TakeComparer())); 
        if (updates.Students != null) oldSession.Students.AddRange(updates?.Students.Except(oldSession.Students));
        if (!string.IsNullOrEmpty(updates.Title)) oldSession.Title = updates.Title;

        _sessionCaching.Create(oldSession);
    }

    public void UpdateSession(Session updates) => 
        _sessionCaching.Create(updates);

    public void AddQuestions(List<Question> questions, Session session)
    {
        session.Questions.AddRange(questions.Select(question => question.Id.ToString()));
        
        AddUpdates(session);
    }

    public List<Session> GetAllSessions()
    {
        return _sessionCaching.GetAllSessions();
    }

    public bool ReplyToQuestion(Session session, User user, string questionId, string answerId)
    {
        Take userTake = session.Takes.Find(take => take.StudentId == user.Id);

        if (userTake.QuestionAnswers == null) 
            userTake.QuestionAnswers = new();

        bool isCorrect = false;
        if (userTake.QuestionAnswers.Any(answer => answer.QuestionId == questionId))
        {
            if (!session.IsAllowedChangedAnswers)
                throw new ArgumentException("В этой сессии нельзя менять ответы");
            
            userTake.QuestionAnswers.RemoveAll(answer => answer.QuestionId == questionId);
            userTake.QuestionAnswers.Add(new QuestionAnswer
            {
                QuestionId = questionId,
                AnswerId = Convert.ToInt32(answerId)
            });
        }
        else
        {
            Question question = _questionsCache.GetQuestion(session.ShortId, questionId);
            isCorrect = question.Answers.First(answer => answer.Id == Convert.ToInt32(answerId)).IsCorrect;

            userTake.QuestionAnswers.Add(new QuestionAnswer
            {
                QuestionId = questionId,
                AnswerId = Convert.ToInt32(answerId),
                IsCorrect = isCorrect
            });   
        }

        UpdateSession(session);
        return isCorrect;
    }

    public void RemoveSession(string shortId)
    {
        _sessionCaching.Delete(shortId);
    }

    public Session? GetSession(string shortId)
    {
        return _sessionCaching.Read(shortId);
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

        _sessionCaching.Delete(sessionId);
        _sessionCaching.Delete(sessionId + "-questions");
    }
}