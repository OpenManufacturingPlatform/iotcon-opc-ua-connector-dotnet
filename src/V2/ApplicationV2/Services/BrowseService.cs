// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUA.Models.Discovery;
using OMP.PlantConnectivity.OpcUA.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    internal sealed class BrowseService : IBrowseService
    {
        private readonly ILogger<BrowseService> logger;

        public BrowseService(ILogger<BrowseService> logger)
        {
            this.logger = logger;
        }

        public async Task<BrowseChildNodesResponse> BrowseChildNodes(IOpcUaSession session, BrowseChildNodesCommand command, CancellationToken cancellationToken)
        {
            var result = await BrowseNodeIdAsync(session, command.BrowseDescription, command.BrowseDepth);
            logger.LogDebug("{discoveredNodes} child nodes discovered of NodeId: [{nodeId}] on Endpoint: {endpointUrl}", result.DiscoveredNodes, command.BrowseDescription.NodeId, command.EndpointUrl);
            var response = new BrowseChildNodesResponse(command, result.BrowsedNode, true);
            return response;
        }

        public async Task<BrowseChildNodesResponseCollection> BrowseNodes(IOpcUaSession session, CancellationToken cancellationToken, int browseDepth = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var browsedNodes = new List<BrowsedNode>();
            var browseDescription = CreateBrowseDescription(ObjectIds.ObjectsFolder, ReferenceTypeIds.HierarchicalReferences);

            var hierarchicalReferences = await session.BrowseAsync(browseDescription, cancellationToken);
            var discoveredNodes = 0;

            foreach (var node in hierarchicalReferences)
            {
                logger.LogTrace("Browsing NodeId: [{nodeId}]", node.NodeId);
                discoveredNodes++;

                var result = await GetChildNodesAsync(session, node, browseDepth);

                if (result != default)
                {
                    discoveredNodes += result.discoveredNodes;
                    browsedNodes.Add(result.BrowsedNode);
                }
            }

            logger.LogDebug("[{discoveredNodes}] nodes discovered on Endpoint: [{endpointUrl}]", discoveredNodes, session.GetBaseEndpointUrl());
            var response = new BrowseChildNodesResponseCollection(session.GetBaseEndpointUrl(), browsedNodes, true);
            return response;
        }

        private async Task<(int discoveredNodes, BrowsedNode BrowsedNode)> GetChildNodesAsync(IOpcUaSession session, ReferenceDescription referenceDescription, int browseDepth = 0)
        {
            var nodeId = ExpandedNodeId.ToNodeId(referenceDescription.NodeId, session.GetNamespaceUris()!);
            var browsedNode = await BrowseNodeIdAsync(session, nodeId, browseDepth);
            return browsedNode;
        }

        private async Task<(int discoveredNodes, BrowsedNode BrowsedNode)> BrowseNodeIdAsync(IOpcUaSession session, NodeId nodeId, int browseDepthLimit, int currentDepth = 0)
        {
            var browseDescription = CreateBrowseDescription(nodeId, ReferenceTypeIds.HierarchicalReferences);
            return await BrowseNodeIdAsync(session, browseDescription, browseDepthLimit, currentDepth);
        }

        private async Task<(int DiscoveredNodes, BrowsedNode BrowsedNode)> BrowseNodeIdAsync(IOpcUaSession session, BrowseDescription browseDescription, int browseDepthLimit, int currentDepth = 0)
        {
            var node = session.ReadNode(browseDescription.NodeId);
            //this.EnrichNode(ref node);//TODO: Verify if this is needed

            if (node is null)
                throw new ArgumentOutOfRangeException(nameof(browseDescription.NodeId), $"{browseDescription.NodeId} could not be found");

            var browsedNode = new BrowsedNode { Node = node };
            var discoveredNodes = 1;

            var children = await session.BrowseAsync(browseDescription);
            if (!children.Any() || currentDepth >= browseDepthLimit)
                return (discoveredNodes, browsedNode);

            foreach (var childReferenceDescription in children.ToArray())
            {
                discoveredNodes++;
                var nodeIdDescription = ExpandedNodeId.ToNodeId(childReferenceDescription.NodeId, session.GetNamespaceUris()!);
                var result = await BrowseNodeIdAsync(session, nodeIdDescription, browseDepthLimit, currentDepth + 1);

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
