// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

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
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.Subscription;
using OMP.Connector.Domain.Schema.Responses.Subscription;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.Subscription
{
    internal class RemoveSubscriptionProvider : SubscriptionProvider<RemoveSubscriptionsRequest, RemoveSubscriptionsResponse>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly int _batchSize;

        public RemoveSubscriptionProvider(
            RemoveSubscriptionsRequest command,
            ISubscriptionRepository subscriptionRepository,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILogger<RemoveSubscriptionProvider> logger)
            : base(command, connectorConfiguration, logger)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._batchSize = this.Settings.OpcUa.SubscriptionBatchSize;
        }

        protected override async Task<string> ExecuteCommandAsync()
        {
            var errorMessages = new List<string>();
            if (!this.Settings.DisableSubscriptionRestoreService)
            {
                if (!this.TryUpdateRestorableSubscriptions())
                    errorMessages.Add("Could not remove subscriptions from data management service.");
            }

            if (!await this.TryRemoveSubscriptionsFromSessionAsync())
                errorMessages.Add("Could not remove subscriptions from OPC UA session.");

            if (!errorMessages.Any())
                this.Logger.Debug($"Removed monitored items from subscription(s) on Endpoint: [{this.EndpointUrl}]");

            return this.GetStatusMessage(errorMessages);
        }

        private bool TryUpdateRestorableSubscriptions()
        {
            bool isSuccess = true;
            try
            {
                isSuccess = this._subscriptionRepository.DeleteMonitoredItems(this.EndpointUrl, this.Command.MonitoredItems);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                this.Logger.Warning(ex, "Unable to update all subscriptions in the subscription repository.");
            }
            return isSuccess;
        }

        private Task<bool> TryRemoveSubscriptionsFromSessionAsync()
        {
            var isSuccess = true;
            try
            {
                var activeSubscriptions = this.GetActiveMonitoredItems();

                foreach (var (subscription, items) in activeSubscriptions)
                {
                    var batchHandler = new BatchHandler<MonitoredItem>(this._batchSize, this.UnsubscribeBatches(subscription));
                    batchHandler.RunBatches(items);
                    this.Logger.Debug($"{items.Count} monitored items were removed from subscription [Id: {subscription.Id}]");
                    if (!subscription.MonitoredItems.Any())
                        Session.RemoveSubscription(subscription);
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                this.Logger.Warning(ex, $"Unable to remove subscriptions from OPC UA server session");
            }
            return Task.FromResult(isSuccess);
        }

        private Action<MonitoredItem[]> UnsubscribeBatches(Opc.Ua.Client.Subscription subscription)
        {
            return monitoredItems =>
            {
                try
                {
                    subscription.RemoveItems(monitoredItems);
                    subscription.ApplyChanges();
                }
                catch (ServiceResultException sre)
                {
                    this.Logger.Error(sre, $"Failed to call ApplyChanges() for batch with {monitoredItems.Count()} items: ");
                    throw;
                }
            };
        }

        private List<(Opc.Ua.Client.Subscription, List<Opc.Ua.Client.MonitoredItem>)> GetActiveMonitoredItems()
        {
            var nodeIds = new List<NodeId>();
            foreach (var monitoredItem in this.Command.MonitoredItems)
                nodeIds.Add(new NodeId(monitoredItem.NodeId));

            return (from subscription in this.Session.Subscriptions
                    select (subscription, (from nodeId in nodeIds
                                           join monitoredItem in subscription.MonitoredItems
                                               on nodeId.ToString() equals monitoredItem.ResolvedNodeId.ToString()
                                           select monitoredItem).ToList()
                        )).ToList();
        }

        protected override void GenerateResult(RemoveSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.RemoveSubscriptions;
            result.Message = message;
        }
    }
}
