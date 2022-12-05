using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Palm.Broker.Rabbit;

public class ReceiveManager : IDisposable
{
    IConnection _connection;
    
    EventingBasicConsumer _consumer;
    
    protected ReceiveManager()
    {
        _connection = RabbitService.GetInstance()
            .GetConnection();
    }

    protected void SetQueueReceive(string queue)
    {
        var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _consumer = new EventingBasicConsumer(channel);
        _consumer.Received += Receive;
    }

    protected virtual void Receive(object? sender, BasicDeliverEventArgs e)
    {
        
    }

    public void Dispose()
    {
        _consumer.Received -= Receive;
        _connection.Dispose();
    }
}