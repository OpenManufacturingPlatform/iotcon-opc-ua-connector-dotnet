using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    [JsonConverter(typeof(SensorPayloadConverter))]
    public class SensorTelemetryPayload
    {
        [JsonProperty("dataSource", Required = Required.Always)]
        [Description("Data Source from which sensor values originate.")]
        public SensorTelemetrySource DataSource { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public ISensorTelemetryPayloadData Data { get; set; }
    }
}