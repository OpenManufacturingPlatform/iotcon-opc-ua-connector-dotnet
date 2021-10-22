using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint
{
    public class ResponseProducer : CustomKafkaProducer<string, CommandResponse>, IResponseProducer
    {
        public ResponseProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<ResponseProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}