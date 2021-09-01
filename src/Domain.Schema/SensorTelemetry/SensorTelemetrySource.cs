using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    public class SensorTelemetrySource : Source
    {
        [JsonProperty("ipAddress", NullValueHandling = NullValueHandling.Ignore)]
        [Description("IP Address of data source")]
        public string IpAddress { get; set; }

        [JsonProperty("endpointUrl", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Endpoint URL of the source")]
        public string EndpointUrl { get; set; }
    }
}