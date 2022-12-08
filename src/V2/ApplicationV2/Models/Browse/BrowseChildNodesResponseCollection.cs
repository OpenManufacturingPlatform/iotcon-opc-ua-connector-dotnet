// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Browse
{
    public record BrowseChildNodesResponseCollection : CommandResult<string, List<BrowsedNode>>
    {
        public BrowseChildNodesResponseCollection(string endPointUrl, List<BrowsedNode> response, bool succeeded)
            : base(endPointUrl, response, succeeded) { }
    }
}
