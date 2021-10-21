using System.ComponentModel;
using OMP.Connector.Domain.Schema.Base;
using OMP.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Domain.Schema.Alarms
{
    [Description("Definition of Sensor Telemetry Messages")]
    public class AlarmMessage : Message<AlarmPayload> { }
}