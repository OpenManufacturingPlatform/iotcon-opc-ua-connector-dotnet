using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Base;

namespace OMP.Connector.Domain.Schema.Responses.Control.Base
{
    public abstract class NodeCommandResponse : CommandResponse
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        [Description("Id of the node that the command applied to")]
        public string NodeId { get; set; }
    }
}