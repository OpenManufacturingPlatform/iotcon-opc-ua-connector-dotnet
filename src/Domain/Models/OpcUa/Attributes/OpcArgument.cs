using Newtonsoft.Json;
using OMP.Connector.Domain.Extensions;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcArgument
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dataType")]
        public OpcNodeId DataType { get; set; }

        [JsonProperty("valueRank")]
        public int ValueRank { get; set; }

        [JsonProperty("arrayDimensions")]
        public uint[] ArrayDimensions { get; set; }

        [JsonProperty("description")]
        public OpcLocalizedText Description { get; set; }

        [JsonProperty("systemDataType")]
        public string SystemTypeString => this.DataType.GetSystemType(this.ValueRank).ToString();
    }
}