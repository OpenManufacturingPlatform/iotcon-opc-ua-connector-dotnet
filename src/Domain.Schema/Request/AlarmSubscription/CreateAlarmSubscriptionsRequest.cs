// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;

namespace OMP.Connector.Domain.Schema.Request.AlarmSubscription
{
    public class CreateAlarmSubscriptionsRequest : AlarmSubscriptionRequest
    {
        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("Monitored items in subscription")]
        public AlarmSubscriptionMonitoredItem[] MonitoredItems { get; set; }
    }
}
