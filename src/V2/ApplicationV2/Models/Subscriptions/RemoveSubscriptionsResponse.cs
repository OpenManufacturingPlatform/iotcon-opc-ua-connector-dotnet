// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Subscriptions
{
    public record RemoveSubscriptionsResponse: CommandResult<RemoveSubscriptionsCommand, RemoveSubscriptionsResponse>
    {
        public List<NodeId> NodesOfRemovedSubscriptions { get; set; } = new List<NodeId>();

        public List<NodeId> NodesWithActiveSubscriptions { get; set; } = new List<NodeId>();
    }        
}
