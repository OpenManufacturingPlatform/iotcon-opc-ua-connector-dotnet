using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua;
using ReadRequest =OMP.Connector.Domain.Schema.Request.Control.ReadRequest;

namespace OMP.Connector.Application.Providers.Commands
{
    public class RegisteredReadProvider: ReadProvider
    {
        public RegisteredReadProvider(
            IEnumerable<ReadRequest> commands,
            IOpcSession opcSession,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMapper mapper,
            ILogger<RegisteredReadProvider> logger): base(commands, opcSession, connectorConfiguration, mapper, logger) {}

        protected override (NodeId NodeId, ReadRequest Command)[] GetNodeIdCommands()
        {
            var registeredNodeIds = this.OpcSession.GetRegisteredNodeIds(this.Commands.Select(x => x.NodeId));
            
            return (from pair in registeredNodeIds
                   join command in this.Commands on pair.Key equals command.NodeId
                   select (pair.Value, command))
                .ToArray();
        }
    }
}