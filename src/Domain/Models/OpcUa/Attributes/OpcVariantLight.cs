using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcVariantLight
    {
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
