namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public interface IOpcSessionReconnectHandlerFactory
    {
        IOpcSessionReconnectHandler Create();
    }
}