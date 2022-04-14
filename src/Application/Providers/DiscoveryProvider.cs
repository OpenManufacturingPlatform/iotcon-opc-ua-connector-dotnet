// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.OpcUa;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema.Request.Discovery;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers
{
    public class DiscoveryProvider : IDiscoveryProvider
    {
        private readonly ILogger _logger;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly IMapper _mapper;
        private long _discoveredNodes;
        private int _browseDepth;
        private Session _session;

        public DiscoveryProvider(IOptions<ConnectorConfiguration> connectorConfiguration, IMapper mapper, ILogger<DiscoveryProvider> logger)
        {
            this._logger = logger;
            this._connectorConfiguration = connectorConfiguration.Value;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<BrowsedNode>> DiscoverRootNodesAsync(IOpcSession opcSession, int browseDepth)
        {
            IEnumerable<BrowsedNode> nodes = default;
            this._browseDepth = browseDepth > 0 ? browseDepth : 0;

            await opcSession.UseAsync((session, complexTypeSystem) =>
            {
                this._session = session;
                this._logger.Debug($"Starting server nodes discovery with Browse Depth: [{this._browseDepth}] " +
                                   $" on Endpoint: [{this._session.ConfiguredEndpoint.EndpointUrl}]");
                nodes = this.GetServerNodes();
            });
            return nodes;
        }

        public async Task<BrowsedNode> DiscoverChildNodesAsync(IOpcSession opcSession, BrowseChildNodesRequest request)
        {
            this._logger.Trace($"Starting child node discovery of NodeId: [{request.NodeId}] with Browse Depth: [{request.BrowseDepth}]");

            var browsedNode = new BrowsedNode();
            await opcSession.UseAsync((session, complexTypeSystem) =>
            {
                this._session = session;
                this._browseDepth = int.Parse(request.BrowseDepth);
                browsedNode = this.BrowseNodeId(request.NodeId);

                this._logger.Debug($"[{this._discoveredNodes}] child nodes discovered of NodeId: [{request.NodeId}] on Endpoint: [{this._session.ConfiguredEndpoint.EndpointUrl}]");
            });

            return browsedNode;
        }

        private IEnumerable<BrowsedNode> GetServerNodes()
        {
            var browsedNodes = new List<BrowsedNode>();

            var hierarchicalReferences = this.Browse(ObjectIds.ObjectsFolder, ReferenceTypeIds.HierarchicalReferences);

            foreach (var node in hierarchicalReferences)
            {
                this._logger.Trace($"Browsing NodeId: [{node.NodeId}] ....");
                this._discoveredNodes++;

                var browsedNode = this.GetChildNodes(node);

                if (browsedNode != default)
                    browsedNodes.Add(browsedNode);
            }

            this._logger.Debug($"[{this._discoveredNodes}] nodes discovered on Endpoint: [{this._session.ConfiguredEndpoint.EndpointUrl}]");

            return browsedNodes;
        }

        private ReferenceDescriptionCollection Browse(NodeId nodeId, NodeId referenceTypeIds)
        {
            var browseDescription = new BrowseDescription
            {
                NodeId = nodeId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = referenceTypeIds,
                IncludeSubtypes = true,
                NodeClassMask = this._connectorConfiguration.OpcUa.NodeMask,
                ResultMask = (uint)BrowseResultMask.All
            };

            var references = ClientSessionUtilities.Browse(this._session, browseDescription, this._logger);

            return references;
        }

        private BrowsedNode GetChildNodes(ReferenceDescription referenceDescription)
        {
            var nodeId = ExpandedNodeId.ToNodeId(referenceDescription.NodeId, this._session.NamespaceUris);
            var browsedNode = this.BrowseNodeId(nodeId);
            return browsedNode;
        }

        private BrowsedNode BrowseNodeId(NodeId nodeId, int childNodeBrowseDepth = 0)
        {
            var node = this._session.NodeCache.FetchNode(nodeId) ?? this._session.ReadNode(nodeId);
            this.EnrichNode(ref node);

            var browsedNode = new BrowsedNode { Node = node };

            var children = this.Browse(nodeId, ReferenceTypeIds.HierarchicalReferences);

            if (!children.Any() || childNodeBrowseDepth >= this._browseDepth)
                return browsedNode;

            foreach (var childReferenceDescription in children.ToArray())
            {
                var nodeIdDescription = ExpandedNodeId.ToNodeId(childReferenceDescription.NodeId, this._session.NamespaceUris);
                var browsedChildNode = this.BrowseNodeId(nodeIdDescription, childNodeBrowseDepth + 1);

                if (browsedChildNode == default) continue;

                this._discoveredNodes++;
                browsedNode.ChildNodes.Add(browsedChildNode);
            }
            return browsedNode;
        }

        private void EnrichNode(ref Node node)
        {
            switch (node)
            {
                case VariableNode variableNode:
                    {
                        var variableNodeWithType = this._mapper.Map<VariableNodeWithType>(variableNode);
                        variableNodeWithType.DataTypeName = ClientSessionUtilities.GetNodeFriendlyDataType(this._session, variableNode.DataType, variableNode.ValueRank);

                        node = variableNodeWithType;
                        break;
                    }
                case VariableTypeNode variableTypeNode:
                    {
                        var variableTypeNodeWithType = this._mapper.Map<VariableTypeNodeWithType>(variableTypeNode);
                        variableTypeNodeWithType.DataTypeName = ClientSessionUtilities.GetNodeFriendlyDataType(this._session, variableTypeNode.DataType, variableTypeNode.ValueRank);

                        node = variableTypeNodeWithType;
                        break;
                    }
            }
        }
    }
}