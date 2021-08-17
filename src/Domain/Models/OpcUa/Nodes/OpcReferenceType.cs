using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcReferenceType : OpcNode
    {
        [JsonProperty("isAbstract")]
        public bool IsAbstract { get; set; }

        [JsonProperty("symmetric")]
        public bool Symmetric { get; set; }

        [JsonProperty("inverseName")]
        public OpcLocalizedText InverseName { get; set; }
    }
}