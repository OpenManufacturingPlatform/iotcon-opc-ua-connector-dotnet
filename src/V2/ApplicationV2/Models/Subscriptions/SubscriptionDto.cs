// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record SubscriptionDto //TODO: Object PostFixed with Dto while others are not - Decide on a standard & implements
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string EndpointUrl { get; set; } = string.Empty;

        public string PublishingInterval { get; set; } = string.Empty;

        public IDictionary<NodeId, SubscriptionMonitoredItem> MonitoredItems { get; set; } = new Dictionary<NodeId, SubscriptionMonitoredItem>();
    }
}
