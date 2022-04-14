// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Base;

namespace OMP.Connector.Domain.Schema.Responses.Subscription
{
    public class CreateSubscriptionsResponse : CommandResponse
    {
        [JsonProperty("monitoredItems", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Monitored items in subscription")]
        public CreateSubscriptionItemResponse[] MonitoredItems { get; set; }
    }
}