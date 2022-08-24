// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record CreateSubscriptionsCommand
    {
        public SubscriptionMonitoredItem[] MonitoredItems { get; set; } = new SubscriptionMonitoredItem[0];
    }


}
