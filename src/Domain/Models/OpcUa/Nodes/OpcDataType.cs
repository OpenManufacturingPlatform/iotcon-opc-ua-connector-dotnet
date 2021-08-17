using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcDataType : OpcNode
    {
        [JsonProperty("isAbstract")]
        public bool IsAbstract { get; set; }
    }
}
