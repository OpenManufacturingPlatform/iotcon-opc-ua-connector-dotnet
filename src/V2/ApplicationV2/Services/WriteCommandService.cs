// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models;
using ApplicationV2.Models.Writes;
using ApplicationV2.Sessions;
using ApplicationV2.Sessions.SessionManagement;
using Opc.Ua;

namespace ApplicationV2.Services
{
    public class WriteCommandService : IWriteCommandService
    {
        private readonly ISessionPoolStateManager sessionPoolStateManager;

        public WriteCommandService(ISessionPoolStateManager sessionPoolStateManager)
        {
            this.sessionPoolStateManager = sessionPoolStateManager;
        }
        public async Task<WriteResponseCollection> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var opcSession = await sessionPoolStateManager.GetSessionAsync(commands.EndpointUrl, cancellationToken);
            var commandsWithRegisteredNodeIds = GetCommandsWithRegisterdNodeIds(opcSession, commands);

            var writeValueCollection = new WriteValueCollection(commandsWithRegisteredNodeIds);
            var responseHeader = opcSession.WriteNodes(writeValueCollection, out var statusCodeCollection);

            //TODO: Retrun errors where WritValue was NULL
            //  - Wat gaan die OpcUaSession doen as ons 'n NULL WriteValue in stuur?? Ek dink ons hoef niks hier te doen nie
            //    Kom ons toets dit sodra die Version ruinning is?

            var writeResults = new WriteResponseCollection();
            writeResults.AddRange(commands.Zip(statusCodeCollection)
                                    .Select(z => new CommandResult<WriteCommand, StatusCode>(z.First, z.Second)));

            return writeResults;
        }

        private IEnumerable<WriteValue> GetCommandsWithRegisterdNodeIds(IOpcUaSession opcSession, WriteCommandCollection commands)
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
