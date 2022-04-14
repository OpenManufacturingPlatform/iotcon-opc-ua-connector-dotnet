// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua;
using WriteRequest = OMP.Connector.Domain.Schema.Request.Control.WriteRequest;

namespace OMP.Connector.Application.Providers.Commands
{
    public class RegisteredWriteProvider : WriteProvider
    {
        public RegisteredWriteProvider(IEnumerable<WriteRequest> commands, IOpcSession opcSession, IMapper mapper, ILogger<RegisteredWriteProvider> logger)
            : base(commands, opcSession, mapper, logger) { }

        protected override (NodeId NodeId, WriteRequest Command)[] GetNodeIdCommands()
        {
            var registeredNodeIds = this.OpcSession.GetRegisteredNodeIds(this.Commands.Select(x => x.NodeId));

            return (from pair in registeredNodeIds
                    join command in this.Commands on pair.Key equals command.NodeId
                    select (pair.Value, command))
                .ToArray();
        }
    }
}