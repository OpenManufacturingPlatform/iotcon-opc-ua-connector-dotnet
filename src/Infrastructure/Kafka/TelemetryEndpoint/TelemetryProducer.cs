using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Common.Producers;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Device.Connector.Kafka.TelemetryEndpoint
{
    public class TelemetryProducer : CustomKafkaProducer<string, SensorTelemetryMessage>, ITelemetryProducer
    {
        public TelemetryProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<TelemetryProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}
