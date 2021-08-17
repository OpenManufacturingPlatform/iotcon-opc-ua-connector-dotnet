using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcTypeInfo
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("typeInfo")]
        public OpcTypeInfo TypeInfo { get; set; }
    }
}