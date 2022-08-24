// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Discovery
{
    public record DiscoveryChildNodesCommand
    {
        public string NodeId { get; set; } = string.Empty;
        public string BrowseDepth { get; set; } = string.Empty;
    }
}
