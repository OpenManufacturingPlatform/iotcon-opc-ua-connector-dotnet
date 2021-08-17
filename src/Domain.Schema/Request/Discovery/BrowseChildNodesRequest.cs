using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Discovery.Base;

namespace Omp.Connector.Domain.Schema.Request.Discovery
{
    public class BrowseChildNodesRequest : DiscoveryRequest
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        [Description("Id of start node for discovery")]
        public string NodeId { get; set; }

        [JsonProperty("browseDepth", Required = Required.Always)]
        [Description("Initial browse depth")]
        public string BrowseDepth { get; set; }
    }
}