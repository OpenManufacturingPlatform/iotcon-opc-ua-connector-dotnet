using System.Collections.Generic;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models
{
    public class AppConfigDto
    {
        public AppConfigDto()
        {
            Subscriptions = new List<SubscriptionDto>();
        }

        [JsonProperty("subscription")]
        public IEnumerable<SubscriptionDto> Subscriptions { get; set; }
    }
}