using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Base;

namespace OMP.Connector.Domain.Schema.Responses.AlarmSubscription
{
    public class CreateAlarmSubscriptionsResponse : CommandResponse
    {
        [JsonProperty("monitoredItems", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Monitored items in subscription")]
        public CreateAlarmSubscriptionItemResponse[] MonitoredItems { get; set; }
    }
}