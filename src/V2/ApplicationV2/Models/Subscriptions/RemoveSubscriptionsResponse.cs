// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record RemoveSubscriptionsResponse : CommandResultBase
    {
        public RemoveSubscriptionsCommand? Command { get; set; } = default;
        public List<NodeId> NodesOfRemovedSubscriptions { get; set; } = new List<NodeId>();
        public List<NodeId> NodesWithActiveSubscriptions { get; set; } = new List<NodeId>();
    }        
}
