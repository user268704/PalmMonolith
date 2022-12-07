using StackExchange.Redis;

namespace Palm.Cash;

public class RedisConnect
{
    static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect($"localhost:49153,password=redispw");
    private IServer _server = _redis.GetServer("localhost:49153");
    static readonly IDatabase _db = _redis.GetDatabase();

    private static RedisConnect? Instance { get; set; }

    private RedisConnect() { }

    public static RedisConnect GetInstance()
    {
        if (Instance == null)
        {
            Instance = new RedisConnect();
        }
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