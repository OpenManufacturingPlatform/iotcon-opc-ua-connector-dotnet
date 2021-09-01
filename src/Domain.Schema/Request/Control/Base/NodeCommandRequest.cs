using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.Base;

namespace OMP.Connector.Domain.Schema.Request.Control.Base
{
    public abstract class NodeCommandRequest : CommandRequest
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        [Description("Id of the node that the command is requested for")]
        public string NodeId { get; set; }
    }
}