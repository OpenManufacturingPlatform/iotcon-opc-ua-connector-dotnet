// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint
{
    public class TelemetryProducer : CustomKafkaProducer<string, SensorTelemetryMessage>, ITelemetryProducer
    {
        public TelemetryProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<TelemetryProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}
