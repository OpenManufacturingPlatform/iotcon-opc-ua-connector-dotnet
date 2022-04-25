// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Responses;
using OMP.Connector.Domain.Schema.Responses.Subscription;

namespace OMP.Connector.Application.Tests.Services
{
    [TestFixture]
    public class ConfigRestoreServiceTests
    {
        public const string NullResponse = nameof(NullResponse);
        public const string NullPayloadResponse = nameof(NullPayloadResponse);
        public const string NullResponseArray = nameof(NullResponseArray);
        public const string EmptyResponseArray = nameof(EmptyResponseArray);
        public const string NonCreateSubscriptionsResponse = nameof(NonCreateSubscriptionsResponse);
        public const string CreateSubscriptionsResponseWithFailStatus = nameof(CreateSubscriptionsResponseWithFailStatus);
        public const string CreateSubscriptionsResponseWithSuccessStatus = nameof(CreateSubscriptionsResponseWithSuccessStatus);

        [Test]
        public async Task Subscriptions_are_read_from_Repository()
        {
            var cancellationToken = new CancellationToken(false);
            var logger = Substitute.For<ILogger<ConfigRestoreService>>();
            var subscriptionRepository = Substitute.For<ISubscriptionRepository>();
            var subscriptionServiceStateManager = Substitute.For<ISubscriptionServiceStateManager>();
            var opcUaConnectorSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            var configRestoreService = new ConfigRestoreService(
                                                                logger,
                                                                subscriptionRepository,
                                                                subscriptionServiceStateManager,
                                                                opcUaConnectorSettings);

            await configRestoreService.RestoreConfigurationAsync(cancellationToken);

            subscriptionRepository
                .Received(1)
                .GetAllSubscriptions();
        }

        [Test]
        public async Task No_Subscriptions_are_restored_when_CancellationToken_is_cancelled()
        {
            var cancellationToken = new CancellationToken(true);
            var logger = Substitute.For<ILogger<ConfigRestoreService>>();
            var subscriptionRepository = Substitute.For<ISubscriptionRepository>();

            subscriptionRepository
                .GetAllSubscriptions()
                .Returns(new SubscriptionDto[]
                {
                    new ()
                });

            var subscriptionServiceStateManager = Substitute.For<ISubscriptionServiceStateManager>();
            var opcUaConnectorSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            var configRestoreService = new ConfigRestoreService(
                logger,
                subscriptionRepository,
                subscriptionServiceStateManager,
                opcUaConnectorSettings);

            Assert.ThrowsAsync<OperationCanceledException>(() => configRestoreService.RestoreConfigurationAsync(cancellationToken));
        }

        [Test]
        public Task Expect_invalid_schema_error_when_no_schema_supplied()
        {
            var cancellationToken = new CancellationToken(false);
            var logger = Substitute.For<ILogger<ConfigRestoreService>>();
            var subscriptionRepository = Substitute.For<ISubscriptionRepository>();

            var subscription = new SubscriptionDto
            {
                EndpointUrl = "opc.tcp://unit-test:52210/"
            };

            subscriptionRepository
                .GetAllSubscriptions()
                .Returns(new[]
                {
                    subscription
                });

            var subscriptionServiceStateManager = Substitute.For<ISubscriptionServiceStateManager>();
            var opcUaConnectorSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            // Interesting if you explicitly make the property NULL you get 'NullReferenceException'.
            // But if you are running on .Net5 and leave the value 'default' then you get a 'ArgumentException'
            opcUaConnectorSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                Communication = new CommunicationConfiguration
                {
                    SchemaUrl = null,
                }
            });

            var configRestoreService = new ConfigRestoreService(
                logger,
                subscriptionRepository,
                subscriptionServiceStateManager,
                opcUaConnectorSettings);

            var argumentException = Assert.ThrowsAsync<NullReferenceException>(() => configRestoreService.RestoreConfigurationAsync(cancellationToken));

            /* Interesting if you explicitly make the property NULL you get 'NullReferenceException'.
               But if you are running on .Net5 and leave the value 'default' then you get a 'ArgumentException'
               var argumentException = await Assert.ThrowsAsync<ArgumentException>(() => configRestoreService.RestoreConfigurationAsync(cancellationToken));
               Assert.Contains("is not a valid Uri", argumentException.Message);
            */

            return Task.CompletedTask;
        }


        [TestCase(NullResponse)]
        [TestCase(NullPayloadResponse)]
        [TestCase(NullResponseArray)]
        [TestCase(EmptyResponseArray)]
        [TestCase(NonCreateSubscriptionsResponse)]
        [TestCase(CreateSubscriptionsResponseWithFailStatus)]
        [TestCase(CreateSubscriptionsResponseWithSuccessStatus)]
        public async Task SubscriptionServiceInstance_is_retrieved_from_ServiceStateManager(string responseState)
        {
            var cancellationToken = new CancellationToken(false);
            var loggerFactory = TestLoggerFactory.Create();
            var logger = loggerFactory.CreateLogger<ConfigRestoreService>();

            var subscriptionRepository = Substitute.For<ISubscriptionRepository>();

            var subscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                EndpointUrl = "opc.tcp://unit-test:52210/"
            };

            var commandResponse = CreateCommandResponse(responseState, subscription.Id);

            subscriptionRepository
                .GetAllSubscriptions()
                .Returns(new[]
                {
                    subscription
                });

            var subscriptionServiceStateManager = Substitute.For<ISubscriptionServiceStateManager>();
            var subscriptionService = Substitute.For<ISubscriptionService>();

            subscriptionServiceStateManager
                .GetSubscriptionServiceInstanceAsync(
                    Arg.Is<string>(p => p.Equals(subscription.EndpointUrl, StringComparison.CurrentCulture)),
                    Arg.Is(cancellationToken))
                .Returns(subscriptionService);


            subscriptionService
                .ExecuteAsync(Arg.Is<CommandRequest>(p => p.Id == subscription.Id))
                .Returns(commandResponse);


            var opcUaConnectorSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaConnectorSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                Communication = new CommunicationConfiguration
                {
                    SchemaUrl = "https://iotpebsdmstorage.blob.core.windows.net/schemas/message_schemas/2020-03-31/bmw.iot.message.opcua.command.request.schema.json",
                }
            });

            var configRestoreService = new ConfigRestoreService(
                logger,
                subscriptionRepository,
                subscriptionServiceStateManager,
                opcUaConnectorSettings);

            await configRestoreService.RestoreConfigurationAsync(cancellationToken);

            await subscriptionServiceStateManager
                .Received(1)
                .GetSubscriptionServiceInstanceAsync(
                    Arg.Is<string>(p => p.Equals(subscription.EndpointUrl, StringComparison.CurrentCulture)),
                    Arg.Is(cancellationToken));

            await subscriptionService
                .Received(1)
                .ExecuteAsync(Arg.Is<CommandRequest>(p => p.Id == subscription.Id));

            await subscriptionServiceStateManager
                .Received(1)
                .CleanupStaleServicesAsync();

            if (!CreateSubscriptionsResponseWithSuccessStatus.Equals(responseState))
            {
                var logEntry1 = loggerFactory.Sink.LogEntries.First();
                Assert.AreEqual(LogLevel.Warning, logEntry1.LogLevel);
                Assert.True(logEntry1.Message?.Contains("ConfigRestoreService failed to restore subscription"));
            }
            var logEntry2 = loggerFactory.Sink.LogEntries.Last();
            Assert.AreEqual(LogLevel.Trace, logEntry2.LogLevel);
            Assert.True(logEntry2.Message?.Contains("ConfigRestoreService finished config restore"));
        }

        private static CommandResponse CreateCommandResponse(string responseState, string requestId)
        {
            return responseState switch
            {
                NullResponse => default,
                NullPayloadResponse => new CommandResponse() { Id = requestId },
                NullResponseArray => new CommandResponse { Id = requestId, Payload = new ResponsePayload { Responses = null } },
                EmptyResponseArray => new CommandResponse
                {
                    Id = requestId,
                    Payload = new ResponsePayload { Responses = new ICommandResponse[0] }
                },
                NonCreateSubscriptionsResponse => new CommandResponse
                {
                    Id = requestId,
                    Payload = new ResponsePayload { Responses = new[] { new RemoveSubscriptionsResponse() } }
                },
                CreateSubscriptionsResponseWithFailStatus => new CommandResponse
                {
                    Id = requestId,
                    Payload = new ResponsePayload
                    {
                        Responses = new[]
                        {
                            new CreateSubscriptionsResponse
                            {
                                Message = "Bad", // TODO: Revisit this
                                StatusCode ="Bad - Irrelevant, not checked"
                            }
                        }
                    }
                },
                CreateSubscriptionsResponseWithSuccessStatus => new CommandResponse
                {
                    Id = requestId,
                    Payload = new ResponsePayload
                    {
                        Responses = new[]
                        {
                            new CreateSubscriptionsResponse
                            {
                                Message = "Some Random text as long as it does not start with 'Bad' - not sure if this is correct", // TODO: Revisit this
                                StatusCode ="Irrelevant - not checked"

                            }
                        }
                    }
                },
                _ => throw new ArgumentException("Unknown response state")
            };
        }
    }
}
