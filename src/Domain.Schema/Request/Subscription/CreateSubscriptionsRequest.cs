// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.Subscription.Base;

namespace OMP.Connector.Domain.Schema.Request.Subscription
{
    public class CreateSubscriptionsRequest : SubscriptionRequest
    {
        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("Monitored items in subscription")]
        public SubscriptionMonitoredItem[] MonitoredItems { get; set; }
    }
}