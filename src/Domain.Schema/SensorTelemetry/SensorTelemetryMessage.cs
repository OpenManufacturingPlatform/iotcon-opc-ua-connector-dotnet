using System.ComponentModel;
using Omp.Connector.Domain.Schema.Base;

namespace Omp.Connector.Domain.Schema.SensorTelemetry
{
    [Description("Definition of Sensor Telemetry Messages")]
    public class SensorTelemetryMessage : Message<SensorTelemetryPayload> { }
}