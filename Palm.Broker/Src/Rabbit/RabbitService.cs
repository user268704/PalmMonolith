using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Palm.Broker.Rabbit;

public class RabbitService : IBroker, IDisposable
{
    readonly ConnectionFactory _factory = new()
    {
        HostName = "localhost"
    };

    readonly IConnection _connection;
    private readonly IModel _model;
    
    // singleton
    private static RabbitService Instance { get; set; }

    private RabbitService()
    {
        _connection = _factory.CreateConnection();
        _model = _connection.CreateModel();
        
        CreateQueues();
    }
    
    public static RabbitService GetInstance()
    {
        if (Instance == null)
        {
            Instance = new RabbitService();
        }

        return Instance;
    }
    
    public IConnection GetConnection()
    {
        return _connection;
    }

    private void CreateQueues()
    {
        _model.QueueDeclare(
            queue: "session",
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        _model.QueueDeclare(
            queue: "session_responses",
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _model.QueueDeclare(
            queue: "errors",
            exclusive: false,
            autoDelete: false,
            arguments: null,
            durable: true
        );
    }
    
    public void RemoveQueues()
    {
        _model.QueueDelete("session");
        _model.QueueDelete("session_responses");
        _model.QueueDelete("errors");
    }
    
    public void Dispose()
    {
        _connection.Dispose();
        _model.Dispose();
    }
    
    public void Publish(string queue, string message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        
        _model.BasicPublish(
            exchange: "",
            routingKey: queue,
            basicProperties: null,
            body: body
        );
    }

    public void Publish(string queue, byte[] message)
    {
        _model.BasicPublish(
            exchange: "",
            routingKey: queue,
            basicProperties: null,
            body: message
        );
    }
}