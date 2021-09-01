using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Device.Connector.Kafka.Common.Producers;

namespace OMP.Device.Connector.Kafka.TelemetryEndpoint
{
    public interface ITelemetryProducer : ICustomKafkaProducer<string, SensorTelemetryMessage> { }
}
