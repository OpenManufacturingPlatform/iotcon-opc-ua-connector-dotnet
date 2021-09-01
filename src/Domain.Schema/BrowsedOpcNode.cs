using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class BrowsedOpcNode : OpcNode
    {
        [JsonProperty("nodeType")] 
        public string NodeType { get; set; }

        [JsonProperty("dataType")] 
        public string DataType { get; set; }

        [JsonProperty("value")] 
        public string Value { get; set; }

        [JsonProperty("valueRank")] 
        public string ValueRank { get; set; }

        [JsonProperty("arrayDimensions")] 
        public ArrayDimensions ArrayDimensions { get; set; }

        [JsonProperty("accessLevel")] 
        public string AccessLevel { get; set; }

        [JsonProperty("userAccessLevel")] 
        public string UserAccessLevel { get; set; }

        [JsonProperty("minimumSamplingInterval")]
        public string MinimumSamplingInterval { get; set; }

        [JsonProperty("historizing")] 
        public bool Historizing { get; set; }

        [JsonProperty("childNodes")] 
        public BrowsedOpcNode[] ChildNodes { get; set; }
    }
}