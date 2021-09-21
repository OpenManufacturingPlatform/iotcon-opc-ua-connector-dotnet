using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Infrastructure.MQTT.Common.Publishers
{
    public interface IMqttResponsePublisher : IMqttPublisher<CommandResponse>
    { }

    public interface IMqttTelemetryPublisher : IMqttPublisher<SensorTelemetryMessage>
    { }

    public interface IMqttPublisher<T>
    {
        Task PublishAsync(T message);
    }
}
