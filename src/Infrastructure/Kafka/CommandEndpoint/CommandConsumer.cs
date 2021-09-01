using Confluent.Kafka;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Consumers;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Device.Connector.Kafka.CommandEndpoint
{
    public class CommandConsumer : CustomKafkaConsumer<string, CommandRequest>, ICommandConsumer
    {
        public CommandConsumer(
            KafkaConfig kafkaConfig,
            ConsumerConfig configuration,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}
