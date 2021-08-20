using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.Commands;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers.Commands;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Control;

namespace OMP.Connector.Application.Factories
{
    public class CommandProviderFactory : ICommandProviderFactory
    {
        private readonly IOptions<ConnectorConfiguration> _connectorConfiguration;
        private readonly IMapper _mapper;
        private readonly ILoggerFactory _loggerFactory;

        public CommandProviderFactory(IOptions<ConnectorConfiguration> connectorConfiguration, IMapper mapper, ILoggerFactory loggerFactory)
        {
            this._connectorConfiguration = connectorConfiguration;
            this._mapper = mapper;
            this._loggerFactory = loggerFactory;
        }

        public IEnumerable<ICommandProvider> GetProcessors(IEnumerable<ICommandRequest> nodeCommands, IOpcSession opcSession)
        {
            return nodeCommands
                       .GroupBy(command => command.GetType())
                       .Select(group => (CommandType: group.Key, Commands: group.ToList()))
                       .Select(cg => this.GetProvider(opcSession, cg.CommandType, cg.Commands))
                       .Where(provider => provider != default)
                       .Select(provider => provider);
        }

        private ICommandProvider GetProvider(IOpcSession opcSession, Type commandType, List<ICommandRequest> commands)
        {
            return commandType switch
            {
                { } when commandType == typeof(BrowseRequest) => this.CreateBrowseProvider(opcSession, commands.Cast<BrowseRequest>()),
                { } when commandType == typeof(CallRequest) => this.CreateCallProvider(opcSession, commands.Cast<CallRequest>()),
                { } when commandType == typeof(ReadRequest) => this.CreateReadProvider(opcSession, commands.Cast<ReadRequest>()),
                { } when commandType == typeof(WriteRequest) => this.CreateWriteProvider(opcSession, commands.Cast<WriteRequest>()),
                _ => default
            };
        }

        private ICommandProvider CreateBrowseProvider(IOpcSession opcSession, IEnumerable<BrowseRequest> commands)
            => new BrowseProvider(commands, opcSession, this._mapper, this._loggerFactory.CreateLogger<BrowseProvider>());

        private ICommandProvider CreateCallProvider(IOpcSession opcSession, IEnumerable<CallRequest> commands)
            => new CallProvider(commands, opcSession, this._mapper, this._loggerFactory.CreateLogger<CallProvider>());

        private ICommandProvider CreateReadProvider(IOpcSession opcSession, IEnumerable<ReadRequest> commands)
        {
            if (this._connectorConfiguration.Value.OpcUa.EnableRegisteredNodes)
                return new RegisteredReadProvider(commands, opcSession, this._connectorConfiguration, this._mapper, this._loggerFactory.CreateLogger<RegisteredReadProvider>());

            return new ReadProvider(commands, opcSession, this._connectorConfiguration, this._mapper, this._loggerFactory.CreateLogger<ReadProvider>());
        }

        private ICommandProvider CreateWriteProvider(IOpcSession opcSession, IEnumerable<WriteRequest> commands)
        {
            if (this._connectorConfiguration.Value.OpcUa.EnableRegisteredNodes)
                return new RegisteredWriteProvider(commands, opcSession, this._mapper, this._loggerFactory.CreateLogger<RegisteredWriteProvider>());

            return new WriteProvider(commands, opcSession, this._mapper, this._loggerFactory.CreateLogger<WriteProvider>());
        }
    }
}