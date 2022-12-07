using System.Text.Json;
using Palm.Models.Cache;
using Palm.Models.Sessions;
using StackExchange.Redis;

namespace Palm.Cash;

public class RedisCache : ISessionСaching
{
    static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect($"localhost:49153,password=redispw");
    private IServer _server = _redis.GetServer("localhost:49153");
    static readonly IDatabase _db = _redis.GetDatabase();
    
    public void AddSession(Session session)
    {
        string value = JsonSerializer.Serialize(session);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Session is not valid", nameof(session));

        _db.StringGetSet(session.ShortId, value);
    }

    public Session GetSession(string id)
    {
        var value = _db.StringGet(id);
        if (value.IsNullOrEmpty)
            throw new ("Session not found");

        return JsonSerializer.Deserialize<Session>(value);
    }

    public bool IsExistStudentInSession(string sessionShortId, string userId)
    {
        string? sessionJson = _db.StringGet(sessionShortId);

        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session not found", nameof(sessionShortId));
        
        Session session = JsonSerializer.Deserialize<Session>(sessionJson);
        
        return session.Students.Any(x => x.ToString() == userId);
    }

    public void Remove(string id)
    {
        _db.KeyDelete(id);
    }

    public void Update(Session oldSession, Session newSession)
    {
        _db.KeyDelete(oldSession.Id.ToString()[..6]);

        string value = JsonSerializer.Serialize(newSession);
        
        _db.StringSet(newSession.ShortId, value);
    }

    public void Update(Session sessionUpdate)
    {
        string? sessionJson = _db.StringGet(sessionUpdate.ShortId);
        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session is not valid", nameof(sessionUpdate));

        Session session = JsonSerializer.Deserialize<Session>(sessionJson);
        _db.KeyDelete(sessionUpdate.ShortId);
        sessionJson = JsonSerializer.Serialize(session);

        _db.StringSet(session.ShortId, sessionJson);
    }

    public List<Session> GetAllSessions()
    {
        var keys = _server.Keys();
        
        List<Session> sessions = new();
        foreach (RedisKey key in keys)
        {
            var value = _db.StringGet(key);
            if (value.IsNullOrEmpty)
                continue;

            sessions.Add(JsonSerializer.Deserialize<Session>(value));
        }

        return sessions;
    }

    public void Clear()
    {
        _server.FlushDatabase();
    }
}