// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.MetaData.Message;
using OMP.Connector.Domain.Schema.Request;
using OMP.Connector.Domain.Schema.Request.Subscription;
using OMP.Connector.Domain.Schema.Responses.Subscription;

namespace OMP.Connector.Application.Services
{
    public class ConfigRestoreService
    {
        private readonly ISubscriptionServiceStateManager _subscriptionServiceStateManager;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger _logger;
        private string _connectorId => this._connectorConfiguration.ConnectorId;
        private string _schemaUrl => this._connectorConfiguration.Communication.SchemaUrl;

        public ConfigRestoreService(
            ILogger<ConfigRestoreService> logger,
            ISubscriptionRepository subscriptionRepository,
            ISubscriptionServiceStateManager subscriptionServiceStateManager,
            IOptions<ConnectorConfiguration> connectorConfiguration)
        {
            this._logger = logger;
            this._subscriptionRepository = subscriptionRepository;
            this._subscriptionServiceStateManager = subscriptionServiceStateManager;
            this._connectorConfiguration = connectorConfiguration.Value;
        }

        public async Task RestoreConfigurationAsync(CancellationToken cancellationToken)
        {
            var allSubscriptions = this._subscriptionRepository.GetAllSubscriptions().ToArray();
            foreach (var subscriptionDto in allSubscriptions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var request = this.BuildSubscriptionRequest(subscriptionDto);
                var success = await this.RestoreSubscriptionsAsync(request, cancellationToken);

                if (!success)
                    this._logger.Warning($"{nameof(ConfigRestoreService)} failed to restore subscription. Id: {subscriptionDto.Id}");
            }
            this._logger.Trace($"{nameof(ConfigRestoreService)} finished config restore for {allSubscriptions.Count()} subscription/s.");
        }

        private CommandRequest BuildSubscriptionRequest(SubscriptionDto subscriptionDto)
        {
            var monitoredItems = subscriptionDto.MonitoredItems?.Values.ToArray();
            var commandRequest = ModelFactory.CreateInstance<CommandRequest>(this._schemaUrl);
            commandRequest.Id = subscriptionDto.Id;

            commandRequest.MetaData = new MessageMetaData
            {
                CorrelationIds = new string[0],
                TimeStamp = DateTime.UtcNow,
                DestinationIdentifiers = new[]{ new Participant
                {
                    Id = this._connectorId,
                    Name = Constants.TelemetrySenderName,
                    Route = string.Empty,
                    Type = ParticipantType.Gateway
                }}
            };

            commandRequest.Payload = new RequestPayload
            {
                RequestTarget = new RequestTarget
                {
                    EndpointUrl = subscriptionDto.EndpointUrl
                },
                Requests = new List<ICommandRequest>
                {
                    new CreateSubscriptionsRequest
                    {
                        OpcUaCommandType = OpcUaCommandType.CreateSubscription,
                        MonitoredItems = monitoredItems
                    }
                }
            };

            return commandRequest;
        }

        private async Task<bool> RestoreSubscriptionsAsync(CommandRequest subscriptionRequest, CancellationToken cancellationToken)
        {
            var opcUaServerUrl = subscriptionRequest.Payload.RequestTarget.EndpointUrl;
            var subscriptionService = await this._subscriptionServiceStateManager.GetSubscriptionServiceInstanceAsync(opcUaServerUrl, cancellationToken);

            var response = await subscriptionService.ExecuteAsync(subscriptionRequest);
            await this._subscriptionServiceStateManager.CleanupStaleServicesAsync();

            var result = response
                ?.Payload
                ?.Responses
                ?.Select(r => r as CreateSubscriptionsResponse)
                ?.SingleOrDefault()
                ?.StatusIsGood();

            return result ?? false;
        }
    }
}