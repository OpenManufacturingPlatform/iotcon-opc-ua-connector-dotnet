// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Interfaces;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class SubscriptionServiceSetup
    {
        internal static SubscriptionService CreateSubscriptionService(
            string schemaUrl = TestConstants.ExpectedSchemaUrl,
            ISessionPoolStateManager sessionPoolStateManagerMock = null,
            bool setupServerDetailsInEndpointRepo = true,
            string expectedServerName = TestConstants.ExpectedServerName,
            string expectedServerRoute = TestConstants.ExpectedServerRoute,
            bool setupSubscriptionProviderFactory = true,
            ICommandResponse expectedResponse = null)
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

            var logger = Substitute.For<ILogger<SubscriptionService>>();

            var sessionPoolStateManager = sessionPoolStateManagerMock ?? Substitute.For<ISessionPoolStateManager>();
            var subscriptionProviderFactoryMock = Substitute.For<ISubscriptionProviderFactory>();
            if (setupSubscriptionProviderFactory)
            {
                var subscriptionProvider = Substitute.For<ISubscriptionProvider>();
                subscriptionProvider
                    .ExecuteAsync(Arg.Any<Session>(),
                        Arg.Any<IComplexTypeSystem>())
                    .Returns(expectedResponse);

                subscriptionProviderFactoryMock
                    .GetProvider(
                        Arg.Any<ICommandRequest>(),
                        Arg.Any<TelemetryMessageMetadata>()
                        )
                    .Returns(subscriptionProvider);
            }


            return new SubscriptionService(
                sessionPoolStateManager,
                opcUaConnectorSettings,
                subscriptionProviderFactoryMock,
                logger
            );
        }
    }
}