// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.AlarmFilter;

namespace OMP.Connector.Domain.Schema
{
    public class AlarmSubscriptionMonitoredItem : OpcUaMonitoredItem
    {
        [JsonProperty("publishingInterval", Required = Required.Always)]
        [Description("Publishing interval")]
        public string PublishingInterval { get; set; }

        [JsonProperty("heartbeatInterval", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Heartbeat interval - should be greater than / equal to publishing interval")]
        public string HeartbeatInterval { get; set; }

        [JsonProperty("alarmTypeNodeIds", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Filter that determines alarm types to include - if null or empty, then the base alarm type is included by default")]
        public string[] AlarmTypeNodeIds { get; set; }
        
        [JsonProperty("alarmfields", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Filter that determines fields to include - if null or empty, then all fields of all alarm types are included by default")]
        public string[] AlarmFields { get; set; }
    }
}
