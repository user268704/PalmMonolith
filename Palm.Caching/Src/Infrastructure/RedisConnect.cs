using StackExchange.Redis;

namespace Palm.Caching.Infrastructure;

public class RedisConnect
{
    private static readonly ConnectionMultiplexer _redis = 
        ConnectionMultiplexer.Connect("localhost:49153,password=redispw");

    private static readonly IDatabase _db = _redis.GetDatabase();
    private readonly IServer _server = _redis.GetServer("localhost:49153");

    private RedisConnect()
    {
    }

    private static RedisConnect? Instance { get; set; }

    public static RedisConnect GetInstance()
    {
        if (Instance == null) Instance = new RedisConnect();
        return Instance;
    }

    public IDatabase GetDatabase()
    {
        return _db;
    }

    public IServer GetServer()
    {
        return _server;
    }

    public ConnectionMultiplexer GetConnection()
    {
        return _redis;
    }
}