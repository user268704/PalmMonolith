using System.Text.Json;
using Palm.Models.Cache;
using Palm.Models.Sessions;
using StackExchange.Redis;

namespace Palm.Cash;

public class SessionCaching : ISessionСaching
{
    private readonly RedisConnect _redis;
    private readonly IDatabase _database;

    public SessionCaching()
    {
        _redis = RedisConnect.GetInstance();
        _database = _redis.GetDatabase();
    }
    
    public void AddSession(Session session)
    {
        string value = JsonSerializer.Serialize(session);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Session is not valid", nameof(session));

        _database.StringGetSet(session.ShortId, value);
    }

    public Session GetSession(string id)
    {
        var value = _database.StringGet(id);
        if (value.IsNullOrEmpty)
            throw new ("Session not found");

        return JsonSerializer.Deserialize<Session>(value);
    }

    public bool IsExistStudentInSession(string sessionShortId, string userId)
    {
        string? sessionJson = _database.StringGet(sessionShortId);

        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session not found", nameof(sessionShortId));
        
        Session session = JsonSerializer.Deserialize<Session>(sessionJson);
        
        return session.Students.Any(x => x.ToString() == userId);
    }

    public void Remove(string id)
    {
        _database.KeyDelete(id);
    }

    public void Update(Session oldSession, Session newSession)
    {
        string value = JsonSerializer.Serialize(newSession);
        
        _database.StringSet(newSession.ShortId, value);
    }

    public void Update(Session sessionUpdate)
    {
        string? sessionJson = _database.StringGet(sessionUpdate.ShortId);
        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session is not valid", nameof(sessionUpdate));

        Session session = JsonSerializer.Deserialize<Session>(sessionJson);
        sessionJson = JsonSerializer.Serialize(session);

        _database.StringSet(session.ShortId, sessionJson);
    }

    public List<Session> GetAllSessions()
    {
        IServer server = _redis.GetServer();
        
        var keys = server.Keys();
        
        List<Session> sessions = new();
        foreach (RedisKey key in keys)
        {
            var value = _database.StringGet(key);
            if (value.IsNullOrEmpty)
                continue;

            sessions.Add(JsonSerializer.Deserialize<Session>(value));
        }

        return sessions;
    }

    public void Clear()
    {
        IServer server = _redis.GetServer();

        server.FlushDatabase();
    }
}