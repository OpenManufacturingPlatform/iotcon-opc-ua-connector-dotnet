// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.PlantConnectivity.OpcUa.Configuration;
using OMP.PlantConnectivity.OpcUa.Models;
using OMP.PlantConnectivity.OpcUa.Models.Reads;
using OMP.PlantConnectivity.OpcUa.Sessions;
using Opc.Ua;
using ReadValueResponse = OMP.PlantConnectivity.OpcUa.Models.Reads.ReadValueResponse;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    internal sealed class ReadCommandService : IReadCommandService
    {
        private readonly OmpOpcUaConfiguration opcUaConfiguration;
        private readonly ILogger<ReadCommandService> logger;

        public ReadCommandService(
            IOptions<OmpOpcUaConfiguration> options,
            ILogger<ReadCommandService> logger)
        {
            this.opcUaConfiguration = options.Value;
            this.logger = logger;
        }

        public  Task<ReadNodeCommandResponseCollection> ReadNodesAsync(IOpcUaSession opcUaSession, ReadNodeCommandCollection commands, CancellationToken cancellationToken)
        {
            var response = new ReadNodeCommandResponseCollection();
            foreach (var command in commands)
            {
                try
                {
                    var node = opcUaSession.ReadNode(command.NodeId);
                    response.Add(new CommandResult<ReadNodeCommand, Node?>(command, node));
                }
                catch (Exception ex)
                {
                    response.Add(new CommandResult<ReadNodeCommand, Node?>
                    {
                        Command = command,
                        Succeeded = false,
                        Message = ex.Demystify().Message
                    });
                }
            }

            return Task.FromResult(response);
        }

        public Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcUaSession, ReadValueCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = new ReadValueResponseCollection();
            
            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcUaSession, commands);
            var nodeIds = commands.Select(x => x.NodeId).ToList();

            var values = new List<object>();
            var errors = new List<ServiceResult>();

            var batchHandler = new BatchHandler<NodeId>(opcUaConfiguration.ReadBatchSize, ReadValuesInBatch(opcUaSession, values, errors));
            batchHandler.RunBatches(nodeIds.ToList());
            logger.LogTrace("Executed a total of {nrOfNodes} read commands. Batch size = {batchSize}", nodeIds.Count, opcUaConfiguration.ReadBatchSize);

            response.AddRange(
                commands.Zip(values.Zip(errors))
                .Select(r => new CommandResult<ReadValueCommand, ReadValueResponse>(
                                r.First, 
                                new ReadValueResponse(r.Second.First, r.Second.Second), 
                                StatusCode.IsGood(r.Second.Second.StatusCode), 
                                StatusCode.IsGood(r.Second.Second.StatusCode) ? String.Empty : r.Second.Second.StatusCode.ToString()
                             )
                       )
                .ToList());

            return Task.FromResult(response);
        }

        private static IEnumerable<ReadValueCommand> GetCommandsWithRegisterdNodeIds(IOpcUaSession opcUaSession, ReadValueCommandCollection commands)
        {
            var results = commands.ToList();

            var registerdReadNodes = commands
                                    .Where(c => c.DoRegisteredRead && !string.IsNullOrWhiteSpace(c.NodeId.ToString()))
                                    .ToList();

            if (registerdReadNodes.Any())
            {
                var registeredNodeIds = opcUaSession.GetRegisteredNodeIds(registerdReadNodes.Select(c => c.NodeId.ToString()));

                foreach (var rn in registerdReadNodes)
                {
                    var registerNodeId = registeredNodeIds.First(n => n.Key == rn.NodeId);
                    rn.NodeId = registerNodeId.Value.ToString();
                }

                results = commands.Where(c => !c.DoRegisteredRead).ToList();
                results.AddRange(registerdReadNodes);
            }

            return results;
        }

        private Action<NodeId[]> ReadValuesInBatch(IOpcUaSession opcUaSession, List<object> values, List<ServiceResult> errors)
        {
            return (items) =>
            {
                logger.LogTrace("Current batch: Reading {items} nodes...", items.Length);
                values.AddRange(opcUaSession.ReadNodeValues(items.ToList(), out var batchErrors));
                errors.AddRange(batchErrors);
            };
        }
    }
}
