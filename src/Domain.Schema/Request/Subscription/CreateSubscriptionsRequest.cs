using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Request.Subscription.Base;

namespace Omp.Connector.Domain.Schema.Request.Subscription
{
    public class CreateSubscriptionsRequest : SubscriptionRequest
    {
        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("Monitored items in subscription")]
        public SubscriptionMonitoredItem[] MonitoredItems { get; set; }
    }
}