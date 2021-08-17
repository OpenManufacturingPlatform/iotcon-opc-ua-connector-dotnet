using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Responses.Base;

namespace Omp.Connector.Domain.Schema.Responses.Subscription
{
    public class CreateSubscriptionsResponse : CommandResponse
    {
        [JsonProperty("monitoredItems", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Monitored items in subscription")]
        public CreateSubscriptionItemResponse[] MonitoredItems { get; set; }
    }
}