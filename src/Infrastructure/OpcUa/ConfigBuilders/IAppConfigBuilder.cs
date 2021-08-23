using Opc.Ua;

namespace OMP.Connector.Infrastructure.Kafka.ConfigBuilders
{
    public interface IAppConfigBuilder
    {
        ApplicationConfiguration Build();
    }
}