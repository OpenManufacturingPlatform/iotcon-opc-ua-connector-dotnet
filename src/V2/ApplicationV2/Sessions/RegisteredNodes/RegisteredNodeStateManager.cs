// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Opc.Ua;

namespace ApplicationV2.Sessions.RegisteredNodes
{
    internal class RegisteredNodeStateManager : IRegisteredNodeStateManager
    {
        private readonly ConcurrentDictionary<string, NodeId> registeredNodes = new ConcurrentDictionary<string, NodeId>();
        private readonly IOpcUaSession opcUaSession;
        private readonly ILogger<RegisteredNodeStateManager> logger;
        private readonly int batchSize;

        public RegisteredNodeStateManager(IOpcUaSession opcSession, int batchSize, ILogger<RegisteredNodeStateManager> logger)
        {
            opcUaSession = opcSession;
            this.batchSize = batchSize;
            this.logger = logger;
        }

        public void RestoreRegisteredNodeIds()
        {
            try
            {
                var nodesToRegister = registeredNodes.Select(node => node.Key);
                registeredNodes.Clear();

                if (nodesToRegister.Any())
                {
                    var nodeIdsToRegister = new NodeIdCollection(nodesToRegister.Select(x => NodeId.Parse(x)));
                    var registeredNodeIds = RegisterNodeIds(nodeIdsToRegister);
                    var mergedList = nodesToRegister.Zip(registeredNodeIds, (i, j) => (i, j));
                    foreach (var (nodeId, registeredNodeId) in mergedList)
                    {
                        registeredNodes.TryAdd(nodeId, registeredNodeId);
                    }
                    logger.LogTrace("Restored {nodesToRegisterCount} registered node ids.", nodesToRegister.Count());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not restore registered node ids: {error}", ex.Message);
            }
        }

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds)
        {
            IEnumerable<KeyValuePair<string, NodeId>> newlyRegisteredNodes = null;

            var existingRegisteredNodes = registeredNodes
                .Distinct()
                .Where(pair => nodeIds.Any(nodeId => nodeId.Equals(pair.Key)))
                .ToList();

            var nodesNotRegistered = nodeIds
                .Distinct()
                .Where(key => !registeredNodes.Any(pair => key.Equals(pair.Key)))
                .ToList();

            if (nodesNotRegistered.Any())
            {
                var nodeIdsToRegister = new NodeIdCollection(nodesNotRegistered.Select(x => NodeId.Parse(x)));
                var registeredNodeIds = RegisterNodeIds(nodeIdsToRegister);
                var mergedList = nodesNotRegistered.Zip(registeredNodeIds, (i, j) => (i, j));
                foreach (var (nodeId, registeredNodeId) in mergedList)
                {
                    registeredNodes.TryAdd(nodeId, registeredNodeId);
                }

                newlyRegisteredNodes = nodesNotRegistered.Zip(registeredNodeIds, (key, registeredNodeId)
                    => new KeyValuePair<string, NodeId>(key, registeredNodeId));
                existingRegisteredNodes.AddRange(newlyRegisteredNodes);
            }
            return existingRegisteredNodes;
        }

        private NodeIdCollection RegisterNodeIds(NodeIdCollection nodeIdsToRegister)
        {
            var registeredNodeIds = new NodeIdCollection();
            try
            {
                var batchHandler = new BatchHandler<NodeId>(batchSize, RegisterBatch(registeredNodeIds));
                batchHandler.RunBatches(nodeIdsToRegister);
                logger.LogInformation("Registered {nodeIdsToRegisterCount} nodes.", nodeIdsToRegister.Count);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not register nodes : {nodes}. Using original nodeIds.", NodeIdsToConcatenatedString(nodeIdsToRegister, ", "));
                registeredNodeIds = nodeIdsToRegister;
            }
            return registeredNodeIds;
        }

        private Action<NodeId[]> RegisterBatch(NodeIdCollection registeredNodeIds)
        {
            return items =>
            {
                try
                {
                    var nodeIdColl = new NodeIdCollection(items);
                    opcUaSession.RegisterNodes(null, nodeIdColl, out var batchRegisteredNodeIds);
                    registeredNodeIds.AddRange(batchRegisteredNodeIds);
                    logger.LogInformation("Registered new nodeIds: {nodes}", NodeIdsToConcatenatedString(batchRegisteredNodeIds, ", "));
                }
                catch (ServiceResultException sre)
                {
                    logger.LogError(sre, "Failed to register nodes for batch with {items} items: ", items.Count());
                    throw;
                }
            };
        }

        private static string NodeIdsToConcatenatedString(NodeIdCollection nodeIds, string delimiter)
        {
            return nodeIds
                .Select(nodeId => nodeId.ToString())
                .Aggregate((current, next) => $"{current}{delimiter}{next}");
        }


        public void Dispose()
        {
            registeredNodes.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
