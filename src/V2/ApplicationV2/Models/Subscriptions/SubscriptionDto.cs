// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record SubscriptionDto //TODO: Object PostFixed with Dto while others are not - Decide on a standard & implements
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string EndpointUrl { get; set; } = string.Empty;

        public string PublishingInterval { get; set; } = string.Empty;

        public IDictionary<string, SubscriptionMonitoredItem> MonitoredItems { get; set; } = new Dictionary<string, SubscriptionMonitoredItem>();
    }

}
