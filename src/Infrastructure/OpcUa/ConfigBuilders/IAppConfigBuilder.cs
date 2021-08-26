using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ConfigBuilders
{
    public interface IAppConfigBuilder
    {
        ApplicationConfiguration Build();
    }
}