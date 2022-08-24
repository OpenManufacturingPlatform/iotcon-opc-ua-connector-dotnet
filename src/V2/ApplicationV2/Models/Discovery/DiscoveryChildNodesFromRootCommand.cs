// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Discovery
{
    public record DiscoveryChildNodesFromRootCommand
    {
        public string BrowseDepth { get; set; } = string.Empty;
    }


}
