// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Discovery
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
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Variable,
                ResultMask = (uint)BrowseResultMask.BrowseName
            };
        }
    }

    public record BrowseChildNodesResponse : CommandResult<BrowseChildNodesCommand, BrowsedNode>
    {
        public BrowseChildNodesResponse(BrowseChildNodesCommand command, BrowsedNode response, bool succeeded)
            : base(command, response, succeeded) { }
    }

    public record BrowseChildNodesResponseCollection : CommandResult<string, List<BrowsedNode>>
    {
        public BrowseChildNodesResponseCollection(string endPointUrl, List<BrowsedNode> response, bool succeeded)
            : base(endPointUrl, response, succeeded) { }
    }

    [Obsolete("Use BrowseChildNodesCommand instead")]
    public record DiscoveryChildNodesCommand : BrowseChildNodesCommand
    {
        public DiscoveryChildNodesCommand(string endpointUrl, BrowseDescription browseDescription, int browseDepth = 1)
        : base(endpointUrl, browseDescription, browseDepth) { }

        public DiscoveryChildNodesCommand(string endpointUrl, NodeId nodeId, int browseDepth = 1)
            : base(endpointUrl, nodeId, browseDepth) { }
    }

    public class BrowsedNode
    {
        public Node Node { get; init; }

        public List<BrowsedNode> ChildNodes { get; init; } = new List<BrowsedNode>();
    }
}
