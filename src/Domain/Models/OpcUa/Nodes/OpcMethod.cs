using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcMethod : OpcNode
    {
        [JsonProperty("executable")]
        public bool Executable { get; set; }

        [JsonProperty("userExecutable")]
        public bool UserExecutable { get; set; }
    }
}
