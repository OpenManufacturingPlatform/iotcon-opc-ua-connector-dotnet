// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Alarms
{
    public record RemoveAlarmSubscriptionsResponse : CommandResultBase
    {
        public RemoveAlarmSubscriptionsCommand? Command { get; set; } = default;
        public List<NodeId> NodesRemovedFromAlarmSubscriptions { get; set; } = new List<NodeId>();
        public List<NodeId> NodesWithActiveAlarmSubscriptions { get; set; } = new List<NodeId>();
    }
}
