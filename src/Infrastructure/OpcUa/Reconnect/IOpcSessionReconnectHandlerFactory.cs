using OMP.Connector.Infrastructure.OpcUa.Reconnect;

namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public interface IOpcSessionReconnectHandlerFactory
    {
        IOpcSessionReconnectHandler Create();
    }
}