using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Producers;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
{
    public interface IConfigurationProducer : ICustomKafkaProducer<string, AppConfigDto> { }
}
