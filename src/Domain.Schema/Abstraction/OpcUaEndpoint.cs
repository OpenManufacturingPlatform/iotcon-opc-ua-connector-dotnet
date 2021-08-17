using System.ComponentModel;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.Abstraction
{
    public class OpcUaEndpoint
    {
        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("Url of the endpoint")]
        public string EndpointUrl { get; set; }

        [JsonProperty("serverCertificate", Required = Required.Always)]
        [Description("Server certificate of the endpoint")]
        public string ServerCertificate { get; set; }

        [JsonProperty("securityLevel", Required = Required.Always)]
        [Description("Security level of the endpoint")]
        public string SecurityLevel { get; set; }

        [JsonProperty("securityMode", Required = Required.Always)]
        [Description("Security mode of the endpoint")]
        public string SecurityMode { get; set; }

        [JsonProperty("securityPolicyUri", Required = Required.Always)]
        [Description("Security policy uri of the endpoint")]
        public string SecurityPolicyUri { get; set; }

        [JsonProperty("transportProfileUri", Required = Required.Always)]
        [Description("Transport profile uri of the endpoint")]
        public string TransportProfileUri { get; set; }
    }
}