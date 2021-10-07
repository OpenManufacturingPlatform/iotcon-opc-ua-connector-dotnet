using System.Collections.Generic;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models
{
    public class AppConfigDto
    {
        [JsonProperty("endpointDescriptions")]
        public IEnumerable<EndpointDescriptionDto> EndpointDescriptions { get; set; }

        [JsonProperty("subscription")]
        public IEnumerable<SubscriptionDto> Subscriptions { get; set; }
    }
}