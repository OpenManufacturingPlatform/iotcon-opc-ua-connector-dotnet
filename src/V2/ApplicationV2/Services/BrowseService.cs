// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUa.Models.Browse;
using OMP.PlantConnectivity.OpcUa.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    internal sealed class BrowseService : IBrowseService
    {
        private readonly ILogger<BrowseService> logger;

        public BrowseService(ILogger<BrowseService> logger)
        {
            this.logger = logger;
        }

        public async Task<BrowseChildNodesResponse> BrowseChildNodes(IOpcUaSession opcUaSession, BrowseChildNodesCommand command, CancellationToken cancellationToken)
        {
            var result = await BrowseNodeIdAsync(opcUaSession, command.BrowseDescription, command.BrowseDepth);
            logger.LogDebug("{discoveredNodes} child nodes discovered of NodeId: [{nodeId}] on Endpoint: {endpointUrl}", result.DiscoveredNodes, command.BrowseDescription.NodeId, command.EndpointUrl);
            var response = new BrowseChildNodesResponse(command, result.BrowsedNode, true);
            return response;
        }

        public async Task<BrowseChildNodesResponseCollection> BrowseNodes(IOpcUaSession opcUaSession, CancellationToken cancellationToken, int browseDepth = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var browsedNodes = new List<BrowsedNode>();
            var browseDescription = CreateBrowseDescription(ObjectIds.ObjectsFolder, ReferenceTypeIds.HierarchicalReferences);

            var hierarchicalReferences = await opcUaSession.BrowseAsync(browseDescription, cancellationToken);
            var discoveredNodes = 0;

            foreach (var node in hierarchicalReferences)
            {
                logger.LogTrace("Browsing NodeId: [{nodeId}]", node.NodeId);
                discoveredNodes++;

                var result = await GetChildNodesAsync(opcUaSession, node, browseDepth);

                if (result != default)
                {
                    discoveredNodes += result.discoveredNodes;
                    browsedNodes.Add(result.BrowsedNode);
                }
            }

            logger.LogDebug("[{discoveredNodes}] nodes discovered on Endpoint: [{endpointUrl}]", discoveredNodes, opcUaSession.GetBaseEndpointUrl());
            var response = new BrowseChildNodesResponseCollection(opcUaSession.GetBaseEndpointUrl(), browsedNodes, true);
            return response;
        }

        private Task<(int discoveredNodes, BrowsedNode BrowsedNode)> GetChildNodesAsync(IOpcUaSession opcUaSession, ReferenceDescription referenceDescription, int browseDepth = 0)
        {
            var nodeId = ExpandedNodeId.ToNodeId(referenceDescription.NodeId, opcUaSession.GetNamespaceUris()!);
            return BrowseNodeIdAsync(opcUaSession, nodeId, browseDepth);
        }

        private Task<(int discoveredNodes, BrowsedNode BrowsedNode)> BrowseNodeIdAsync(IOpcUaSession opcUaSession, NodeId nodeId, int browseDepthLimit, int currentDepth = 0)
        {
            var browseDescription = CreateBrowseDescription(nodeId, ReferenceTypeIds.HierarchicalReferences);
            return BrowseNodeIdAsync(opcUaSession, browseDescription, browseDepthLimit, currentDepth);
        }

        private async Task<(int DiscoveredNodes, BrowsedNode BrowsedNode)> BrowseNodeIdAsync(IOpcUaSession opcUaSession, BrowseDescription browseDescription, int browseDepthLimit, int currentDepth = 0)
        {
            var node = opcUaSession.ReadNode(browseDescription.NodeId);
            //this.EnrichNode(ref node);//TODO: Verify if this is needed

            if (node is null)
                throw new ArgumentOutOfRangeException(nameof(browseDescription.NodeId), $"{browseDescription.NodeId} could not be found");

            var browsedNode = new BrowsedNode { Node = node };
            var discoveredNodes = 1;

            var children = await opcUaSession.BrowseAsync(browseDescription);
            if (!children.Any() || currentDepth >= browseDepthLimit)
                return (discoveredNodes, browsedNode);

            foreach (var childReferenceDescription in children.ToArray())
            {
                discoveredNodes++;
                var nodeIdDescription = ExpandedNodeId.ToNodeId(childReferenceDescription.NodeId, opcUaSession.GetNamespaceUris()!);
                var result = await BrowseNodeIdAsync(opcUaSession, nodeIdDescription, browseDepthLimit, currentDepth + 1);

                if (result == default)
                    continue;

                discoveredNodes += result.discoveredNodes;
                browsedNode.ChildNodes.Add(result.BrowsedNode);
            }

            return (discoveredNodes, browsedNode);
        }

        private BrowseDescription CreateBrowseDescription(NodeId nodeId, NodeId referenceTypeIds, uint nodeMask = IBrowseService.NodeMask)
            => new BrowseDescription
            {
                NodeId = nodeId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = referenceTypeIds,
                IncludeSubtypes = true,
                NodeClassMask = nodeMask,
                ResultMask = (uint)BrowseResultMask.All
            };
    }
}
