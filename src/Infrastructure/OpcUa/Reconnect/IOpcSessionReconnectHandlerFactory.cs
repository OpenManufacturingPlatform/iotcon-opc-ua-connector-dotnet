namespace OMP.Connector.Infrastructure.Kafka.Reconnect
{
    public interface IOpcSessionReconnectHandlerFactory
    {
        IOpcSessionReconnectHandler Create();
    }
}