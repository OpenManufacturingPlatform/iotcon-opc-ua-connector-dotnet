// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Services;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Browse
{
    public record BrowseChildNodesCommand
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public virtual BrowseDescription BrowseDescription { get; init; }
        public int BrowseDepth { get; set; } = 1;

        public BrowseChildNodesCommand(string endpointUrl, BrowseDescription browseDescription, int browseDepth = 1)
        {
            EndpointUrl = endpointUrl;
            BrowseDepth = browseDepth;
            BrowseDescription = browseDescription;
        }

        public BrowseChildNodesCommand(string endpointUrl, NodeId nodeId, int browseDepth = 1)
        {
            EndpointUrl = endpointUrl;
            BrowseDepth = browseDepth;
            BrowseDescription = new BrowseDescription
            {
                NodeId = nodeId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = IBrowseService.NodeMask,
                ResultMask = (uint)BrowseResultMask.BrowseName
            };
        }
    }

    public class BrowsedNode
    {
        public Node Node { get; init; }

        public List<BrowsedNode> ChildNodes { get; init; } = new List<BrowsedNode>();
    }
}
