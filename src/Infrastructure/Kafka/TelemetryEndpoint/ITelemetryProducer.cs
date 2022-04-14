// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;

namespace OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint
{
    public interface ITelemetryProducer : ICustomKafkaProducer<string, SensorTelemetryMessage> { }
}
