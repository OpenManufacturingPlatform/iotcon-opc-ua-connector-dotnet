using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Control.Base;

namespace OMP.Connector.Domain.Schema.Responses.Control
{
    public class BrowseResponse : NodeCommandResponse
    {
        [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Node")]
        public BrowsedOpcNode Node { get; set; }
    }
}