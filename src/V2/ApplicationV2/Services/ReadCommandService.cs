// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models;
using ApplicationV2.Models.Reads;
using ApplicationV2.Sessions;
using Opc.Ua;
using ReadValueResponse = ApplicationV2.Models.Reads.ReadValueResponse;

namespace ApplicationV2.Services
{
    public class ReadCommandService : IReadCommandService
    {
        public Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcSession, ReadValueCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = new ReadValueResponseCollection();
            //TODO: Talk about error handling -> NULL NodeId            
            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcSession, commands);
            var nodeIds = commands.Select(x => x.NodeId).ToList();
            var values = opcSession.ReadNodes(nodeIds, 10, out var errors);

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
