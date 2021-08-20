using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.Subscription.Base;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using Omp.Connector.Domain.Schema.Enums;
using Omp.Connector.Domain.Schema.Request.Subscription;
using Omp.Connector.Domain.Schema.Responses.Subscription;

namespace OMP.Connector.Application.Providers.Subscription
{
    public class RemoveAllSubscriptionProvider : SubscriptionProvider<RemoveAllSubscriptionsRequest, RemoveAllSubscriptionsResponse>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public RemoveAllSubscriptionProvider(
            RemoveAllSubscriptionsRequest command,
            ISubscriptionRepository subscriptionRepository,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILogger<RemoveAllSubscriptionProvider> logger)
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
                var activeSubscriptions = this.Session.Subscriptions.ToList();
                foreach (var subscription in activeSubscriptions)
                {
                    subscription.SetPublishingMode(false);
                    this.Logger.Trace($"Disabled publishing for subscription [Id: {subscription.Id}]");
                }
                this.Session.RemoveSubscriptions(activeSubscriptions);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                this.Logger.Warning(ex, "Unable to remove all subscriptions from OPC UA server session");
            }
            return Task.FromResult(isSuccess);
        }
        
        protected override void GenerateResult(RemoveAllSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.RemoveAllSubscriptions;
            result.Message = message;
        }
    }
}