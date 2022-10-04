// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Alarms
{
    public record AlarmSubscriptionDto //TODO: Object PostFixed with Dto while others are not - Decide on a standard & implements
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string EndpointUrl { get; set; } = string.Empty;

        public string PublishingInterval { get; set; } = string.Empty;

        public IDictionary<NodeId, AlarmSubscriptionMonitoredItem> AlarmMonitoredItems { get; set; } = new Dictionary<NodeId, AlarmSubscriptionMonitoredItem>();
    }
}
