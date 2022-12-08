// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using System.ComponentModel;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Alarms
{
    public record AlarmSubscriptionMonitoredItem
    {
        public NodeId NodeId { get; set; } = string.Empty;
        public int PublishingInterval { get; set; } = 1000;//TODO: remove from here
        public int HeartbeatInterval { get; set; } = 1000;

        public string[] AlarmTypeNodeIds { get; set; } = Array.Empty<string>();
        public string[] AlarmFields { get; set; } = Array.Empty<string>();
        public EventSeverity Severity = EventSeverity.Min;
        public bool IgnoreSuppressedOrShelved = true;

        public NodeId[] GetAlarmTypesAsNodeIds()
        {
            if (this.AlarmTypeNodeIds == null || !this.AlarmTypeNodeIds.Any())
                return new NodeId[] { ObjectTypeIds.AlarmConditionType };

            return this.AlarmTypeNodeIds.Select(alarmTypeNodeId => NodeId.Parse(alarmTypeNodeId)).ToArray();
        }
    }
}
