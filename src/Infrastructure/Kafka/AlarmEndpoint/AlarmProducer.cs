// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.AlarmEndpoint
{
    public class AlarmProducer : CustomKafkaProducer<string, AlarmMessage>, IAlarmProducer
    {
        public AlarmProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<AlarmProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }
    }
}
