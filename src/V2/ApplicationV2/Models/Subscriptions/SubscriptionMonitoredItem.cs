// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record SubscriptionMonitoredItem
    {
        public NodeId NodeId { get; set; } = string.Empty;
        public int SamplingInterval { get; set; } = 1000;
        public int PublishingInterval { get; set; } = 1000;//TODO: remove from here
        public int HeartbeatInterval { get; set; } = 1000;
        public uint AttributeId { get; set; } = Attributes.Value;
        public MonitoringMode MonitoringMode { get; set; } = MonitoringMode.Reporting;
        public uint QueueSize { get; set; } = 1;
        public bool DiscardOldest { get; set; } = false;
    }
}
