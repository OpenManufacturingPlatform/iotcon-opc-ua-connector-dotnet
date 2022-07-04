// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Infrastructure.MQTT.Common.Publishers
{
    public interface IMqttResponsePublisher : IMqttPublisher<CommandResponse>
    { }

    public interface IMqttTelemetryPublisher : IMqttPublisher<SensorTelemetryMessage>
    { }

    public interface IMqttAlarmPublisher : IMqttPublisher<AlarmMessage>
    { }

    public interface IMqttPublisher<T>
    {
        Task PublishAsync(T message);
    }
}
