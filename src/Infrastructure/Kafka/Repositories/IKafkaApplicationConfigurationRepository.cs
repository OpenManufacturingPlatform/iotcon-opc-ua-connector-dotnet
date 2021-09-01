using OMP.Connector.Domain.Models;

namespace OMP.Device.Connector.Kafka.Repositories
{
    public interface IKafkaApplicationConfigurationRepository
    {
        void Initialize(AppConfigDto applicationConfig);
    }
}