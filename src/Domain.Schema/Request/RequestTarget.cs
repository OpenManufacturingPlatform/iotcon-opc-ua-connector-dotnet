using System.ComponentModel;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.Request
{
    public class RequestTarget
    {
        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("Base url of the OPC UA server")]
        public string EndpointUrl { get; set; }
    }
}