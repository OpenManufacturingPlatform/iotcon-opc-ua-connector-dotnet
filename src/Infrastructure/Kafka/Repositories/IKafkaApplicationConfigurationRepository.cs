using Confluent.Kafka;
using OMP.Connector.Domain.Models;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{
    public interface IKafkaApplicationConfigurationRepository
    {
        void Initialize(AppConfigDto applicationConfig);

        void OnConfigChangeReceived(ConsumeResult<string, AppConfigDto> consumeResult);
    }
}