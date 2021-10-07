using Confluent.Kafka;
using OMP.Connector.Domain.Models;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint
{
    public class ConfigurationConsumer : CustomKafkaConsumer<string, AppConfigDto>, IConfigurationConsumer
    {
        public ConfigurationConsumer(
            KafkaConfig kafkaConfig,
            ConsumerConfig configuration,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}
