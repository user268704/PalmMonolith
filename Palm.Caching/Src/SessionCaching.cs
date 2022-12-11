using System.Text.Json;
using Palm.Abstractions.Interfaces.Caching;
using Palm.Caching.Infrastructure;
using Palm.Models.Sessions;
using StackExchange.Redis;

namespace Palm.Caching;

/// <summary>
///     CRUD operations for sessions
/// </summary>
public class SessionCaching : ISessionСaching
{
    private readonly IDatabase _database;
    private readonly RedisConnect _redis;

    public SessionCaching()
    {
        _redis = RedisConnect.GetInstance();
        _database = _redis.GetDatabase();
    }

    public void Create(Session session)
    {
        var value = JsonSerializer.Serialize(session);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Session is not valid", nameof(session));

        _database.StringGetSet(session.ShortId, value);
    }

    public Session? Read(string id)
    {
        var value = _database.StringGet(id);
        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Session>(value);
    }

    public bool IsExistStudentInSession(string sessionShortId, string userId)
    {
        string? sessionJson = _database.StringGet(sessionShortId);

        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session not found", nameof(sessionShortId));

        var session = JsonSerializer.Deserialize<Session>(sessionJson);

        return session.Students.Any(x => x.ToString() == userId);
    }

    public void Delete(string id)
    {
        _database.KeyDelete(id);
        _database.KeyDelete(id + "-questions");
    }

    public void Update(Session sessionUpdate)
    {
        string? sessionJson = _database.StringGet(sessionUpdate.ShortId);
        if (string.IsNullOrEmpty(sessionJson))
            throw new ArgumentException("Session is not valid", nameof(sessionUpdate));

        var session = JsonSerializer.Deserialize<Session>(sessionJson);
        sessionJson = JsonSerializer.Serialize(session);

        _database.StringSet(session.ShortId, sessionJson);
    }

    public List<Session> GetAllSessions()
    {
        var server = _redis.GetServer();

        var keys = server
            .Keys()
            .Where(item => item.ToString().Length == 6);

        List<Session> sessions = new();
        foreach (var key in keys)
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
        var server = _redis.GetServer();

        server.FlushDatabase();
    }
}