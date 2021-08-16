using System.ComponentModel;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.Responses
{
    public class ResponseSource
    {
        [JsonProperty("id", Required = Required.Always)]
        [Description("Id of the source where the command responses are being sent from")]
        public string Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        [Description("Name of the source where the command responses are being sent from")]
        public string Name { get; set; }

        [JsonProperty("route", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Route of the source")]
        public string Route { get; set; }
        
        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("Endpoint URL of the source")]
        public string EndpointUrl { get; set; }
    }
}