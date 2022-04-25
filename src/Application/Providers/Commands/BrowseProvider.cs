// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application.Providers.Commands.Base;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Domain.Schema.Responses.Control;

namespace OMP.Connector.Application.Providers.Commands
{
    public class BrowseProvider : CommandProvider<BrowseRequest, BrowseResponse>
    {
        public BrowseProvider(IEnumerable<BrowseRequest> commands, IOpcSession opcSession, IMapper mapper, ILogger<BrowseProvider> logger)
            : base(commands, opcSession, mapper, logger) { }

        protected override async Task<IEnumerable<BrowseResponse>> RunCommandAsync()
        {

            var nodeIdCommands = this.GetNodeIdCommands();
            var browsedResults = await this.OpcSession.BrowseNodesAsync(nodeIdCommands, this.ConstructResult);
            return browsedResults;
        }
    }
}