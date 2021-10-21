using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;

namespace OMP.Connector.Domain.Schema.Request.AlarmSubscription
{
    public class RemoveAlarmSubscriptionsRequest : AlarmSubscriptionRequest
    {
        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("Monitored items in subscription")]
        public OpcUaMonitoredItem[] MonitoredItems { get; set; }
    }
}