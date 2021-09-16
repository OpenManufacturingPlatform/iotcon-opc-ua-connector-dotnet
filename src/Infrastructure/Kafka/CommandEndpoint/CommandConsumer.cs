using Confluent.Kafka;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.CommandEndpoint
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
