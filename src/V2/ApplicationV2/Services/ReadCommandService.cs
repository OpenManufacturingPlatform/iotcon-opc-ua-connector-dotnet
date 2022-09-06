// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using OMP.PlantConnectivity.OpcUA.Models;
using OMP.PlantConnectivity.OpcUA.Models.Reads;
using OMP.PlantConnectivity.OpcUA.Sessions;
using Opc.Ua;
using ReadValueResponse = OMP.PlantConnectivity.OpcUA.Models.Reads.ReadValueResponse;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    public class ReadCommandService : IReadCommandService
    {
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

        public Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcSession, ReadValueCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = new ReadValueResponseCollection();
            //TODO: Talk about error handling -> NULL NodeId            
            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcSession, commands);
            var nodeIds = commands.Select(x => x.NodeId).ToList();
            var values = opcSession.ReadNodeValues(nodeIds, 10, out var errors);

            response.AddRange(
                commands.Zip(values.Zip(errors))
                .Select(r => new CommandResult<ReadValueCommand, ReadValueResponse>
                            (r.First, new ReadValueResponse(r.Second.First, r.Second.Second)))
                .ToList());

            return Task.FromResult(response);
        }

        private static IEnumerable<ReadValueCommand> GetCommandsWithRegisterdNodeIds(IOpcUaSession opcSession, ReadValueCommandCollection commands)
        {
            var results = commands.ToList();

            var registerdReadNodes = commands
                                    .Where(c => c.DoRegisteredRead && !string.IsNullOrWhiteSpace(c.NodeId.ToString()))//TODO: Change the ToString().
                                    .ToList();

            if (registerdReadNodes.Any())
            {
                var registeredNodeIds = opcSession.GetRegisteredNodeIds(registerdReadNodes.Select(c => c.NodeId.ToString()));//TODO: Change the ToString().

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
    }
}
