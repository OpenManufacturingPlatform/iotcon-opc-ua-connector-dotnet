using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.Subscription.Base;

namespace OMP.Connector.Domain.Schema.Request.Subscription
{
    public class RemoveSubscriptionsRequest : SubscriptionRequest
    {
        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("Monitored items in subscription")]
        public OpcUaMonitoredItem[] MonitoredItems { get; set; }
    }
}