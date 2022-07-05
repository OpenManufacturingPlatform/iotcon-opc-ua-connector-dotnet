// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.AlarmSubscription.Base;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;
using OMP.Connector.Domain.Schema.Responses.AlarmSubscription;

namespace OMP.Connector.Application.Providers.AlarmSubscription
{
    public class RemoveAllAlarmSubscriptionProvider : AlarmSubscriptionProvider<RemoveAllAlarmSubscriptionsRequest, RemoveAllAlarmSubscriptionsResponse>
    {
        private readonly IAlarmSubscriptionRepository _subscriptionRepository;

        public RemoveAllAlarmSubscriptionProvider(
            RemoveAllAlarmSubscriptionsRequest command,
            IAlarmSubscriptionRepository subscriptionRepository,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILogger<RemoveAllAlarmSubscriptionProvider> logger)
            : base(command, connectorConfiguration, logger)
        {
            this._subscriptionRepository = subscriptionRepository;
        }

        protected override async Task<string> ExecuteCommandAsync()
        {
            var errorMessages = new List<string>();
            if (!this.Settings.DisableSubscriptionRestoreService)
            {
                if (!this.TryRemoveRestorableSubscriptions())
                {
                    errorMessages.Add("Could not remove all subscriptions from data management service.");
                }
            }
            if (!await this.TryRemoveSubscriptionFromSessionAsync())
            {
                errorMessages.Add("Could not remove all subscriptions from OPC UA session.");
            }

            if (!errorMessages.Any())
                this.Logger.Debug($"Removed all subscriptions on Endpoint: [{this.EndpointUrl}]");

            return this.GetStatusMessage(errorMessages);
        }

        private bool TryRemoveRestorableSubscriptions()
        {
            var isSuccess = true;
            try
            {
                var subscriptions = this._subscriptionRepository.GetAllByEndpointUrl(this.EndpointUrl);
                foreach (var subscription in subscriptions)
                {
                    isSuccess = this._subscriptionRepository.Remove(subscription) && isSuccess;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                this.Logger.Warning(ex, "Unable to remove all subscriptions from the subscription repository.");
            }
            return isSuccess;
        }

        private Task<bool> TryRemoveSubscriptionFromSessionAsync()
        {
            var isSuccess = true;
            try
            {
                var activeSubscriptions = this.OpcSession.Session.Subscriptions.ToList();
                foreach (var subscription in activeSubscriptions)
                {
                    subscription.SetPublishingMode(false);
                    this.Logger.Trace($"Disabled publishing for subscription [Id: {subscription.Id}]");
                }
                this.OpcSession.Session.RemoveSubscriptions(activeSubscriptions);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                this.Logger.Warning(ex, "Unable to remove all subscriptions from OPC UA server session");
            }
            return Task.FromResult(isSuccess);
        }

        protected override void GenerateResult(RemoveAllAlarmSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.RemoveAllAlarmSubscriptions;
            result.Message = message;
        }
    }
}
