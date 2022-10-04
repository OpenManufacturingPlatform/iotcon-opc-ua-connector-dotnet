// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Browse
{
    public record BrowseChildNodesResponse : CommandResult<BrowseChildNodesCommand, BrowsedNode>
    {
        public BrowseChildNodesResponse(BrowseChildNodesCommand command, BrowsedNode response, bool succeeded)
            : base(command, response, succeeded) { }
    }
}
