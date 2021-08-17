using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.Request
{
    public class RequestPayload
    {
        [JsonProperty("requestTarget", Required = Required.Always)]
        [Description("Target for executing command requests")]
        public RequestTarget RequestTarget { get; set; }

        [JsonProperty("requests", Required = Required.Always)]
        [Description("Requests to execute command/s")]
        public IEnumerable<ICommandRequest> Requests { get; set; }
    }
}