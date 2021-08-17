using Newtonsoft.Json;
using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcNodeId
    {
        [JsonIgnore]
        public ushort NamespaceIndex { get; set; }

        [JsonIgnore]
        public IdType IdType { get; set; }

        [JsonIgnore]
        public object Identifier { get; set; }

        [JsonIgnore]
        public bool IsNullNodeId { get; set; }

        [JsonProperty("nodeId")]
        public string FriendlyName { get; set; }
    }
}
