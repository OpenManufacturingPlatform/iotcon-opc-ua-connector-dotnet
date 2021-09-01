using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcVariableType : OpcNode
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("dataType")]
        public OpcNodeId DataType { get; set; }

        [JsonProperty("valueRank")]
        public int ValueRank { get; set; }

        [JsonProperty("arrayDimensions")]
        public uint[] ArrayDimensions { get; set; }

        [JsonProperty("isAbstract")]
        public bool IsAbstract { get; set; }

        [JsonProperty("dataTypeName")]
        public string DataTypeName { get; set; }
    }
}