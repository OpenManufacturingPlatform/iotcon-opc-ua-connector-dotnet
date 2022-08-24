// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Infrastructure.OpcUa.States
{
    public class RegisteredNodeStateManager : IRegisteredNodeStateManager
    {
        private readonly ConcurrentDictionary<string, NodeId> _registeredNodes;
        private Session _session;
        private readonly ILogger _logger;
        private readonly int _batchSize;

        public RegisteredNodeStateManager(Session session, ILogger<RegisteredNodeStateManager> logger, int registerNodeBatchSize)
        {
            _registeredNodes = new ConcurrentDictionary<string, NodeId>();
            _session = session;
            _logger = logger;
            _batchSize = registerNodeBatchSize;
        }

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds)
        {
            IEnumerable<KeyValuePair<string, NodeId>> newlyRegisteredNodes = null;

            var existingRegisteredNodes = _registeredNodes
                .Distinct()
                .Where(pair => nodeIds.Any(nodeId => nodeId.Equals(pair.Key)))
                .ToList();

            var nodesNotRegistered = nodeIds
                .Distinct()
                .Where(key => !_registeredNodes.Any(pair => key.Equals(pair.Key)))
                .ToList();

            if (nodesNotRegistered.Any())
            {
                var nodeIdsToRegister = new NodeIdCollection(nodesNotRegistered.Select(x => NodeId.Parse(x)));
                var registeredNodeIds = RegisterNodeIds(nodeIdsToRegister);
                var mergedList = nodesNotRegistered.Zip(registeredNodeIds, (i, j) => (i, j));
                foreach (var (nodeId, registeredNodeId) in mergedList)
                {
                    _registeredNodes.TryAdd(nodeId, registeredNodeId);
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
                var batchHandler = new BatchHandler<NodeId>(_batchSize, RegisterBatch(registeredNodeIds));
                batchHandler.RunBatches(nodeIdsToRegister);
                _logger.Information($"Registered {nodeIdsToRegister.Count} nodes.");
            }
            catch (Exception e)
            {
                _logger.LogTrace(e, $"Could not register nodes : {NodeIdsToConcatenatedString(nodeIdsToRegister, ", ")}. Using original nodeIds.");
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
                    _session.RegisterNodes(null, nodeIdColl, out NodeIdCollection batchRegisteredNodeIds);
                    registeredNodeIds.AddRange(batchRegisteredNodeIds);
                    _logger.Trace($"Registered new nodeIds: {NodeIdsToConcatenatedString(batchRegisteredNodeIds, ", ")}");
                }
                catch (ServiceResultException sre)
                {
                    _logger.Error(sre, $"Failed to register nodes for batch with {items.Count()} items: ");
                    throw;
                }
            };
        }

        private string NodeIdsToConcatenatedString(NodeIdCollection nodeIds, string delimiter)
        {
            return nodeIds
                .Select(nodeId => nodeId.ToString())
                .Aggregate((current, next) => $"{current}{delimiter}{next}");
        }

        public void RestoreRegisteredNodeIds(Session session)
        {
            try
            {
                _session = session;

                var nodesToRegister = _registeredNodes.Select(node => node.Key);
                _registeredNodes.Clear();

                if (nodesToRegister.Any())
                {
                    var nodeIdsToRegister = new NodeIdCollection(nodesToRegister.Select(x => NodeId.Parse(x)));
                    var registeredNodeIds = RegisterNodeIds(nodeIdsToRegister);
                    var mergedList = nodesToRegister.Zip(registeredNodeIds, (i, j) => (i, j));
                    foreach (var (nodeId, registeredNodeId) in mergedList)
                    {
                        _registeredNodes.TryAdd(nodeId, registeredNodeId);
                    }
                    _logger.Trace($"Restored {nodesToRegister.Count()} registered node ids.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Could not restore registered node ids: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _registeredNodes.Clear();
        }
    }
}
