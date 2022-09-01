// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Discovery
{
    public record DiscoveryChildNodesCommand
    {
        public virtual NodeId NodeId { get; set; } = string.Empty;
        public string BrowseDepth { get; set; } = string.Empty;//TODO: should this be string?
    }
}
