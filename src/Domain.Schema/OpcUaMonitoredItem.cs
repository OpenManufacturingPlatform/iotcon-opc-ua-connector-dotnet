using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema
{
    public class OpcUaMonitoredItem
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        public string NodeId { get; set; }
    }
}