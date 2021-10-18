using System.Collections.Generic;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models
{
    public class AppConfigDto
    {
        public AppConfigDto()
        {
            EndpointDescriptions = new List<EndpointDescriptionDto>();
            Subscriptions = new List<SubscriptionDto>();
        }

        [JsonProperty("endpointDescriptions")]
        public IEnumerable<EndpointDescriptionDto> EndpointDescriptions { get; set; }

        [JsonProperty("subscription")]
        public IEnumerable<SubscriptionDto> Subscriptions { get; set; }
    }
}