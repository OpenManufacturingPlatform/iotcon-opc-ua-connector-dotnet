using System.Collections.Generic;
using Newtonsoft.Json;
using OMP.Connector.Domain.Json;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes.Base
{
    [JsonConverter(typeof(OpcNodeConverter))]
    public abstract class OpcNode
    {
        [JsonProperty("nodeId")]
        public OpcNodeId NodeId { get; set; }

        [JsonProperty("nodeClass")]
        public NodeClass NodeClass { get; set; }

        [JsonProperty("browseName")]
        public OpcQualifiedName BrowseName { get; set; }

        [JsonProperty("displayName")]
        public OpcLocalizedText DisplayName { get; set; }

        [JsonProperty("description")]
        public OpcLocalizedText Description { get; set; }

        [JsonProperty("writeMask")]
        public uint WriteMask { get; set; }

        [JsonProperty("userWriteMask")]
        public uint UserWriteMask { get; set; }

        [JsonProperty("childNodes")]
        public IEnumerable<OpcNode> ChildNodes { get; set; }
    }
}
