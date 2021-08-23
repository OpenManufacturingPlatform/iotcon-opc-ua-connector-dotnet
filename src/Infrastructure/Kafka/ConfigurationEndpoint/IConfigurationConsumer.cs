using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Consumers;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
{
    public interface IConfigurationConsumer: ICustomKafkaConsumer<string, AppConfigDto>
    {

    }
}
