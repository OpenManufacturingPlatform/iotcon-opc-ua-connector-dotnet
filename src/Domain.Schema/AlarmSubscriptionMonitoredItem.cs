// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class AlarmSubscriptionMonitoredItem : OpcUaMonitoredItem
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
    }
}
