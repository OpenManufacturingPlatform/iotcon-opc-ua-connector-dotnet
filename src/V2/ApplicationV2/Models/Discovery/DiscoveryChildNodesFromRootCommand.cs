// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Discovery
{
    public record DiscoveryChildNodesFromRootCommand
    {
        public string BrowseDepth { get; set; } = string.Empty;
    }
}
