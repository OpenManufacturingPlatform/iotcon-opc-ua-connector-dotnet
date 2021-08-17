using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcVariable : OpcNode
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("dataType")]
        public OpcNodeId DataType { get; set; }

        [JsonProperty("valueRank")]
        public int ValueRank { get; set; }

        [JsonProperty("arrayDimensions")]
        public uint[] ArrayDimensions { get; set; }

        [JsonProperty("accessLevel")]
        public byte AccessLevel { get; set; }

        [JsonProperty("userAccessLevel")]
        public byte UserAccessLevel { get; set; }

        [JsonProperty("minimumSamplingInterval")]
        public double MinimumSamplingInterval { get; set; }

        [JsonProperty("historizing")]
        public bool Historizing { get; set; }

        [JsonProperty("dataTypeName")]
        public string DataTypeName { get; set; }
    }
}