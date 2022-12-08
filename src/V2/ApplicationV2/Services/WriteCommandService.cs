// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Linq;
using OMP.PlantConnectivity.OpcUa.Models;
using OMP.PlantConnectivity.OpcUa.Models.Writes;
using OMP.PlantConnectivity.OpcUa.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    internal sealed class WriteCommandService : IWriteCommandService
    {
        public Task<WriteResponseCollection> WriteAsync(IOpcUaSession opcUaSession, WriteCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcUaSession, commands);

            var writeValueCollection = new WriteValueCollection(commandsWithRegisteredNodeIds);
            var responseHeader = opcUaSession.WriteNodes(writeValueCollection, out var statusCodeCollection);

            //TODO: Retrun errors where WritValue was NULL
            //  - Wat gaan die OpcUaSession doen as ons 'n NULL WriteValue in stuur?? Ek dink ons hoef niks hier te doen nie
            //    Kom ons toets dit sodra die Version ruinning is?

            var writeResults = new WriteResponseCollection();
            writeResults.AddRange(commands.Zip(statusCodeCollection)
                                    .Select(z => new CommandResult<WriteCommand, StatusCode>(
                                        z.First,
                                        z.Second,
                                        StatusCode.IsNotBad(z.Second),
                                        StatusCode.IsNotGood(z.Second) ? z.Second.ToString() : string.Empty)));

            return Task.FromResult(writeResults);
        }

        private static IEnumerable<WriteValue> GetCommandsWithRegisterdNodeIds(IOpcUaSession opcUaSession, WriteCommandCollection commands)
        {
            var results = commands.ToList();

            var registerdWriteNodes = commands
                                    .Where(c => c.DoRegisteredWrite && c.Value is not null)
                                    .ToList();

            if (registerdWriteNodes.Any())
            {
                var registeredNodeIds = opcUaSession.GetRegisteredNodeIds(registerdWriteNodes.Select(c => c.Value!.NodeId.ToString()));

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
