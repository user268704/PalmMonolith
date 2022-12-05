namespace Palm.Broker;

public interface IBroker
{
    public void Publish(string queue, string message);
    public void Publish(string queue, byte[] message);
}