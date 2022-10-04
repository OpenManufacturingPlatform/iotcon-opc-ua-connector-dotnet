// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Subscriptions
{
    public record SubscriptionMonitoredItem
    {
        public NodeId NodeId { get; set; } = string.Empty;
        public int SamplingInterval { get; set; } = 1000;

        //TODO: Move to subscription - requires schema changes
        public int PublishingInterval { get; set; } = 1000;

        //TODO: Move to subscription - makes no sense on monitoredItem, requires schema changes
        public uint KeepAliveCount { get; set; } = 1000; //Replaces heartbeatinterval which does not exist in OPC UA specification

        public uint AttributeId { get; set; } = Attributes.Value;
        public MonitoringMode MonitoringMode { get; set; } = MonitoringMode.Reporting;
        public uint QueueSize { get; set; } = 1;
        public bool DiscardOldest { get; set; } = false;
        public byte Priority { get; set; } = 0;
    }
}
