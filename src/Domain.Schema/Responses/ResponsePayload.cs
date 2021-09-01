using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Responses
{
    public class ResponsePayload
    {
        [JsonProperty("responseStatus", Required = Required.Always)]
        [Description("Status of the request execution")]
        public OpcUaResponseStatus ResponseStatus { get; set; }

        [JsonProperty("responseSource", Required = Required.Always)]
        [Description("Source from where command response is sent")]
        public ResponseSource ResponseSource { get; set; }
        
        [JsonProperty("responses", Required = Required.Always)]
        [Description("Responses to command request/s")]
        public IEnumerable<ICommandResponse> Responses { get; set; }
    }
}