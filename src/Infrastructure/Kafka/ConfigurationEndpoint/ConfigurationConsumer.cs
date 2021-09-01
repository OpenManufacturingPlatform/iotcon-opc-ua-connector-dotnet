using Confluent.Kafka;
using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Consumers;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
{
    public class ConfigurationConsumer:  CustomKafkaConsumer<string, AppConfigDto>, IConfigurationConsumer
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
