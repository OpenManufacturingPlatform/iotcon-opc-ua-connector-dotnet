using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Discovery.Base;

namespace Omp.Connector.Domain.Schema.Request.Discovery
{
    public class ServerDiscoveryRequest : DiscoveryRequest
    {
        [JsonProperty("serverDetails", Required = Required.Always)]
        [Description("Details of server to discover")]
        public ServerDetails ServerDetails { get; set; }
    }
}