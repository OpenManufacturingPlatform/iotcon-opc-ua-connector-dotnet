using OMP.Connector.Domain.Models;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{
    public interface IKafkaApplicationConfigurationRepository
    {
        void Initialize(AppConfigDto applicationConfig);
    }
}