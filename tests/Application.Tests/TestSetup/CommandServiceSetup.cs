// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class CommandServiceSetup
    {
        internal static CommandService CreateCommandService(
            string schemaUrl = TestConstants.ExpectedSchemaUrl,
            ISessionPoolStateManager sessionPoolStateManagerMock = null,
            ICommandProviderFactory commandProcessorFactory = null,
            string expectedServerName = TestConstants.ExpectedServerName,
            string expectedServerRoute = TestConstants.ExpectedServerRoute,
            bool setupServerDetailsInEndpointRepo = true)
        {
            if (string.IsNullOrWhiteSpace(schemaUrl))
                schemaUrl = TestConstants.ExpectedSchemaUrl;

            var opcUaConnectorSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaConnectorSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                Communication = new CommunicationConfiguration
                {
                    SchemaUrl = schemaUrl,
                }
            });

            var sessionPoolStateManager = sessionPoolStateManagerMock ?? Substitute.For<ISessionPoolStateManager>();

            var logger = Substitute.For<ILogger<CommandService>>();
            commandProcessorFactory ??= Substitute.For<ICommandProviderFactory>();

            var commandService = new CommandService(
                commandProcessorFactory,
                sessionPoolStateManager,
                opcUaConnectorSettings,
                logger
            );

            return commandService;
        }
    }
}
