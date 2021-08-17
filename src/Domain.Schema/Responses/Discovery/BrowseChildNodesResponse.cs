using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Responses.Discovery.Base;

namespace Omp.Connector.Domain.Schema.Responses.Discovery
{
    public class BrowseChildNodesResponse : DiscoveryResponse
    {
        [JsonProperty("node", Required = Required.Always)]
        [Description("Start node for discovery")]
        public DiscoveredOpcNode DiscoveredOpcNode { get; set; }
    }
}