using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Responses.Control.Base;

namespace Omp.Connector.Domain.Schema.Responses.Control
{
    public class BrowseResponse : NodeCommandResponse
    {
        [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Node")]
        public BrowsedOpcNode Node { get; set; }
    }
}