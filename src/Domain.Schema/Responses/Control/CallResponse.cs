using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Responses.Control.Base;

namespace Omp.Connector.Domain.Schema.Responses.Control
{
    public class CallResponse : NodeCommandResponse
    {
        [JsonProperty("arguments", Required = Required.Always)]
        [Description("Output arguments")]
        public IEnumerable<OutputArgument> Arguments { get; set; }
    }
}