using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;

namespace OMP.Connector.Domain.Schema.Alarms
{
    [JsonConverter(typeof(AlarmPayloadConverter))]
    public class AlarmPayload
    {
        [JsonProperty("dataSource", Required = Required.Always)]
        [Description("Data Source from which sensor values originate.")]
        public AlarmSource DataSource { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public AlarmEventData Data { get; set; }
    }
}