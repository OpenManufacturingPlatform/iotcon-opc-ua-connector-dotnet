using OMP.Connector.Infrastructure.Kafka.CommandEndpoint;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers
{
    public interface IConsumerFactory
    {
        IConfigurationConsumer CreateConfigurationConsumer();
        ICommandConsumer CreateCommandConsumer();
    }
}
