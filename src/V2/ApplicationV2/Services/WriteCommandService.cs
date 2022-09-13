// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models;
using OMP.PlantConnectivity.OpcUA.Models.Writes;
using OMP.PlantConnectivity.OpcUA.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    internal sealed class WriteCommandService : IWriteCommandService
    {
        public Task<WriteResponseCollection> WriteAsync(IOpcUaSession opcSession, WriteCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcSession, commands);

            var writeValueCollection = new WriteValueCollection(commandsWithRegisteredNodeIds);
            var responseHeader = opcSession.WriteNodes(writeValueCollection, out var statusCodeCollection);

            //TODO: Retrun errors where WritValue was NULL
            //  - Wat gaan die OpcUaSession doen as ons 'n NULL WriteValue in stuur?? Ek dink ons hoef niks hier te doen nie
            //    Kom ons toets dit sodra die Version ruinning is?

            var writeResults = new WriteResponseCollection();
            writeResults.AddRange(commands.Zip(statusCodeCollection)
                                    .Select(z => new CommandResult<WriteCommand, StatusCode>(z.First, z.Second)));

            return Task.FromResult(writeResults);
        }

        private static IEnumerable<WriteValue> GetCommandsWithRegisterdNodeIds(IOpcUaSession opcSession, WriteCommandCollection commands)
        {
            var results = commands.ToList();

            var registerdWriteNodes = commands
                                    .Where(c => c.DoRegisteredWrite && c.Value is not null)
                                    .ToList();

            if (registerdWriteNodes.Any())
            {
                var registeredNodeIds = opcSession.GetRegisteredNodeIds(registerdWriteNodes.Select(c => c.Value!.NodeId.ToString()));

                foreach (var rn in registerdWriteNodes)
                {
                    var registerNodeId = registeredNodeIds.First(n => n.Key == rn.Value!.NodeId);
                    rn.Value!.NodeId = registerNodeId.Value;
                }

                results = commands.Where(c => !c.DoRegisteredWrite).ToList();
                results.AddRange(registerdWriteNodes);
            }

            return results.Where(c => c.Value is not null).Select(c => c.Value!);
        }
    }
}
