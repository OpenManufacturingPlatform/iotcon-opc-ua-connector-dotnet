// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;

namespace OMP.Connector.Domain.Schema.Request.AlarmSubscription
{
    public class CreateAlarmSubscriptionsRequest : AlarmSubscriptionRequest
    {
        [JsonProperty("alarmMonitoredItems", Required = Required.Always)]
        [Description("Alarm monitored items in subscription")]
        public AlarmSubscriptionMonitoredItem[] AlarmMonitoredItems { get; set; }
    }
}
