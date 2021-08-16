using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Discovery.Base;

namespace Omp.Connector.Domain.Schema.Request.Discovery
{
    public class BrowseChildNodesFromRootRequest : DiscoveryRequest
    {
        [JsonProperty("browseDepth", Required = Required.Always)]
        [Description("Initial browse depth")]
        public string BrowseDepth { get; set; }
    }
}