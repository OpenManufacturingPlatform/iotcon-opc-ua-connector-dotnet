using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Control.Base;

namespace Omp.Connector.Domain.Schema.Request.Control
{
    public class CallRequest : NodeCommandRequest
    {
        [JsonProperty("arguments", Required = Required.Always)]
        [Description("Input arguments")]
        public IEnumerable<InputArgument> Arguments { get; set; }
    }
}