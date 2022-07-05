// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class AlarmSubscriptionServiceSetup
    {
        internal static AlarmSubscriptionService CreateAlarmSubscriptionService(
            string schemaUrl = TestConstants.ExpectedSchemaUrl,
            ISessionPoolStateManager sessionPoolStateManagerMock = null,
            bool setupAlarmSubscriptionProviderFactory = true,
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

            var logger = Substitute.For<ILogger<AlarmSubscriptionService>>();

            var sessionPoolStateManager = sessionPoolStateManagerMock ?? Substitute.For<ISessionPoolStateManager>();
            var alarmSubscriptionProviderFactoryMock = Substitute.For<IAlarmSubscriptionProviderFactory>();
            if (setupAlarmSubscriptionProviderFactory)
            {
                var alarmSubscriptionProvider = Substitute.For<IAlarmSubscriptionProvider>();
                alarmSubscriptionProvider
                    .ExecuteAsync(Arg.Any<IOpcSession>())
                    .Returns(expectedResponse);

                alarmSubscriptionProviderFactoryMock
                    .GetProvider(
                        Arg.Any<ICommandRequest>(),
                        Arg.Any<TelemetryMessageMetadata>()
                        )
                    .Returns(alarmSubscriptionProvider);
            }


            return new AlarmSubscriptionService(
                sessionPoolStateManager,
                opcUaConnectorSettings,
                alarmSubscriptionProviderFactoryMock,
                logger
            );
        }
    }
}
