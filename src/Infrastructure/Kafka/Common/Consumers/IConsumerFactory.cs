using OMP.Device.Connector.Kafka.CommandEndpoint;
using OMP.Device.Connector.Kafka.ConfigurationEndpoint;

namespace OMP.Device.Connector.Kafka.Common.Consumers
{
    public interface IConsumerFactory
    {
        IConfigurationConsumer CreateConfigurationConsumer();
        ICommandConsumer CreateCommandConsumer();
    }
}
