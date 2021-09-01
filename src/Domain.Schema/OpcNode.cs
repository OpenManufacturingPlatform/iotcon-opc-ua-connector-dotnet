using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class OpcNode
    {
        [JsonProperty("nodeClass")] 
        public string NodeClass { get; set; }

        [JsonProperty("browseName")] 
        public string BrowseName { get; set; }

        [JsonProperty("displayName")] 
        public string DisplayName { get; set; }

        [JsonProperty("description")] 
        public string Description { get; set; }

        [JsonProperty("writeMask")] 
        public string WriteMask { get; set; }

        [JsonProperty("userWriteMask")] 
        public string UserWriteMask { get; set; }
    }
}