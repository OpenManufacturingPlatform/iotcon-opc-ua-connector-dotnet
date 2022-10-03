// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Browse
{
    public record BrowseChildNodesFromRootCommand
    {
        public string BrowseDepth { get; set; } = string.Empty;
    }
}
