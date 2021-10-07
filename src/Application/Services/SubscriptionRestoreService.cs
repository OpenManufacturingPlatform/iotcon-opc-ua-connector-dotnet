using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Factories;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Request;
using OMP.Connector.Domain.Schema.Request.Subscription;

namespace OMP.Connector.Application.Services
{
    public class SubscriptionRestoreService : ISubscriptionRestoreService
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionProviderFactory _subscriptionProviderFactory;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private string _schemaUrl => this._connectorConfiguration.Communication.SchemaUrl;

        public SubscriptionRestoreService(
            ISubscriptionProviderFactory subscriptionProviderFactory,
            ISubscriptionRepository subscriptionRepository,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILogger<SubscriptionRestoreService> logger)
        {
            this._logger = logger;
            this._subscriptionRepository = subscriptionRepository;
            this._connectorConfiguration = connectorConfiguration.Value;
            this._subscriptionProviderFactory = subscriptionProviderFactory;
        }

        private async Task ExecuteSubscriptionRequestAsync(IOpcSession opcSession, CommandRequest requestMessage)
        {
            try
            {
                await opcSession.UseAsync((session, complexTypeSystem) =>
                {
                    foreach (var request in requestMessage.Payload.Requests)
                    {
                        var provider = this._subscriptionProviderFactory.GetProvider(request, TelemetryMessageMetadata.MapFrom(requestMessage));
                        if (provider == default)
                            throw new ApplicationException("Subscription restore command is not supported");

                        provider.ExecuteAsync(session, complexTypeSystem).GetAwaiter().GetResult();
                    }
                });
            }
            catch (Exception exception)
            {
                this._logger.Error(exception, "Subscription restore command failed with error");
            }
        }

        public async Task RestoreSubscriptionsAsync(IOpcSession opcSession)
        {
            if (this._connectorConfiguration.DisableSubscriptionRestoreService)
                return;

            this._logger.Debug("Restoring subscriptions...");

            SubscriptionDto[] monitoredItems = default;
            try
            {
                monitoredItems = this.GetSubscriptions();
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Could not retrieve subscriptions.");
            }

            var restoredCount = 0;
            if (monitoredItems != default && monitoredItems.Any())
            {
                try
                {
                    await this.RestoreSubscriptionsAsync(opcSession, monitoredItems);
                    restoredCount = monitoredItems.Length;
                }
                catch (Exception ex)
                {
                    this._logger.Error(ex, "Could not restore subscriptions");
                }
            }

            this._logger.Information($"Restored [{restoredCount}] subscriptions");
        }

        private SubscriptionDto[] GetSubscriptions()
        {
            var subscriptions = this._subscriptionRepository.GetAllSubscriptions();
            return subscriptions.ToArray();
        }

        private async Task RestoreSubscriptionsAsync(IOpcSession opcSession, IEnumerable<SubscriptionDto> subscriptions)
        {
            foreach (var currentSubscription in subscriptions)
            {
                var request = ConstructCommandRequest(currentSubscription, this._schemaUrl);
                await this.ExecuteSubscriptionRequestAsync(opcSession, request);
            }
        }

        private static CommandRequest ConstructCommandRequest(SubscriptionDto subscription, string schemaUrl)
        {
            var request = new CreateSubscriptionsRequest
            {
                OpcUaCommandType = OpcUaCommandType.CreateSubscription,
                MonitoredItems = subscription.MonitoredItems?.Values.ToArray()
            };

            var commandRequest = ModelFactory.CreateInstance<CommandRequest>(schemaUrl);
            commandRequest.Payload = new RequestPayload
            {
                RequestTarget = new RequestTarget { EndpointUrl = subscription.EndpointUrl },
                Requests = new List<ICommandRequest> { request }
            };

            return commandRequest;
        }
    }
}