using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Common.Producers;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Device.Connector.Kafka.ResponsesEndpoint
{
    public class ResponseProducer : CustomKafkaProducer<string, CommandResponse>, IResponseProducer
    {
        public ResponseProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<ResponseProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}