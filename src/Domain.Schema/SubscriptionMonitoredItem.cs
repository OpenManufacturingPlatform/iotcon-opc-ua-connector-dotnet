using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class SubscriptionMonitoredItem : OpcUaMonitoredItem
    {
        [JsonProperty("samplingInterval", Required = Required.Always)]
        [Description("Sampling interval - should be less than / equal to publishing interval")]
        public string SamplingInterval { get; set; }

        [JsonProperty("publishingInterval", Required = Required.Always)]
        [Description("Publishing interval")]
        public string PublishingInterval { get; set; }

        [JsonProperty("heartbeatInterval", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Heartbeat interval - should be greater than / equal to publishing interval")]
        public string HeartbeatInterval { get; set; }

        [Obsolete("This property is not in use.", false)]
        [JsonProperty("skipFirst", NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        [Description("Skip the first notification after creation of the subscription")]
        public bool SkipFirst { get; set; }
    }
}
