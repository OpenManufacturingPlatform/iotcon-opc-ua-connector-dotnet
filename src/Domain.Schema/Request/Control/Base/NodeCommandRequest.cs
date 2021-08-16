using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.Base
{
    public abstract class NodeCommandRequest : CommandRequest
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        [Description("Id of the node that the command is requested for")]
        public string NodeId { get; set; }
    }
}