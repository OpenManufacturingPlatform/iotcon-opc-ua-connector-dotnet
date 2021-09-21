using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Responses.Base
{
    public abstract class CommandResponse : Command, ICommandResponse
    {
        [JsonProperty("message", Required = Required.Always)]
        [Description("Status and information about execution of the command is provided in message")]
        public string Message { get; set; }

        [JsonProperty("errorSource", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Source component where the error response was sent from")]
        public string ErrorSource { get; set; }

        [JsonProperty("statusCode", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Code related to the errorSource describing the reason for the error response")]
        public string StatusCode { get; set; }
    }
}