// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record CreateSubscriptionsCommand
    {
        public string EndpointUrl { get; set; } = string.Empty;

        public List<SubscriptionMonitoredItem> MonitoredItems { get; set; } = new List<SubscriptionMonitoredItem>();
    }
}
