using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers.Commands;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Control.Base;
using OMP.Connector.Domain.Schema.Responses.Control.Base;
using Opc.Ua;

namespace OMP.Connector.Application.Providers.Commands.Base
{
    public abstract class CommandProvider<TCommand, TResult> : ICommandProvider
        where TCommand : NodeCommandRequest, ICommandRequest
        where TResult : NodeCommandResponse, ICommandResponse, new()
    {
        protected readonly IEnumerable<TCommand> Commands;
        protected readonly IOpcSession OpcSession;
        protected readonly IMapper Mapper;
        protected readonly ILogger Logger;

        protected CommandProvider(IEnumerable<TCommand> commands, IOpcSession opcSession, IMapper mapper, ILogger logger)
        {
            this.Logger = logger;
            this.Mapper = mapper;
            this.OpcSession = opcSession;
            this.Commands = commands;
        }

        public async Task<IEnumerable<ICommandResponse>> ExecuteAsync()
        {
            if (!this.Commands.Any())
                return default;

            return await this.RunCommandAsync();
        }

        protected abstract Task<IEnumerable<TResult>> RunCommandAsync();

        protected virtual (NodeId NodeId, TCommand Command)[] GetNodeIdCommands()
            => this.Commands.Select(x => (NodeId.Parse(x.NodeId), x)).ToArray();

        protected TResult ConstructResult(NodeCommandRequest nodeCommand)
        {
            var result = (TResult)Activator.CreateInstance(typeof(TResult));
            result!.NodeId = nodeCommand.NodeId;
            return result;
        }
    }
}