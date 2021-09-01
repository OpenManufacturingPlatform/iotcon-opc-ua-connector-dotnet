using System.Collections.Generic;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class DiscoveredOpcNode : OpcNode
    {
        [JsonProperty("nodeId")] 
        public string NodeId { get; set; }

        [JsonProperty("childNodes")] 
        public IEnumerable<DiscoveredOpcNode> ChildNodes { get; set; }
    }
}