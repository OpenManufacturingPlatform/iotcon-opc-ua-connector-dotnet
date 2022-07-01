// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.AlarmSubscription.Base;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;
using OMP.Connector.Domain.Schema.Responses.AlarmSubscription;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.AlarmSubscription
{
    public class CreateAlarmSubscriptionProvider : AlarmSubscriptionProvider<CreateAlarmSubscriptionsRequest, CreateAlarmSubscriptionsResponse>
    {
        //TODO: add support for alarm subscription restores
        private readonly IAlarmSubscriptionRepository _subscriptionRepository;
        private readonly TelemetryMessageMetadata _messageMetadata;
        private readonly AlarmMonitoredItemValidator _alarmMonitoredItemValidator;
        private readonly int _batchSize;
        private readonly IOpcAlarmMonitoredItemService _opcAlarmMonitoredItemService;
        private readonly Dictionary<string, List<string>> _groupedItemsNotCreated;
        private AlarmFilterDefinition m_filter;

        public CreateAlarmSubscriptionProvider(
            IAlarmSubscriptionRepository subscriptionRepository,
            ILogger<CreateAlarmSubscriptionProvider> logger,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcAlarmMonitoredItemService opcAlarmMonitoredItemService,
            CreateAlarmSubscriptionsRequest command,
            TelemetryMessageMetadata messageMetadata,
            AlarmMonitoredItemValidator alarmMonitoredItemValidator) : base(command, connectorConfiguration, logger)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._opcAlarmMonitoredItemService = opcAlarmMonitoredItemService;
            this._messageMetadata = messageMetadata;
            this._alarmMonitoredItemValidator = alarmMonitoredItemValidator;
            this._batchSize = this.Settings.OpcUa.AlarmSubscriptionBatchSize;

            this._groupedItemsNotCreated = new Dictionary<string, List<string>>();
        }

        protected override async Task<string> ExecuteCommandAsync()
        {
            var errorMessages = await this.AddValidationErrorsAsync();
            if (errorMessages.Any())
            {
                this.Logger.Debug($"Validation of {nameof(CreateAlarmSubscriptionsRequest.AlarmMonitoredItems)} was not successful.");
                this.Logger.Trace(string.Join(" ", errorMessages));
                return this.GetStatusMessage(errorMessages);
            }

            try
            {
                var subscriptionGroups = this.Command.AlarmMonitoredItems.GroupBy(item => item.PublishingInterval);

                foreach (var group in subscriptionGroups)
                {
                    var groupItems = group.ToList();
                    var batchHandler = new BatchHandler<AlarmSubscriptionMonitoredItem>(this._batchSize, this.SubscribeBatch());
                    batchHandler.RunBatches(groupItems);
                    this.Logger.Trace($"Alarm subscription with publishing interval {group.Key} ms: Subscribed to {groupItems.Count} nodes.");
                }

                foreach (var sub in this.Session.Subscriptions.Where(sub => !sub.PublishingEnabled))
                {
                    this.Logger.Trace($"Enabling publishing for alarm subscription {sub.Id}");
                    sub.SetPublishingMode(true);
                    sub.ConditionRefresh();
                }
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                errorMessages.Add($"Bad: Could not create / update alarm subscriptions. {error.Message}");
            }

            //TODO: Implement subscription restore for alarms. Requires different Dto
            //if (!base.Settings.DisableSubscriptionRestoreService && !errorMessages.Any())
            //{
            //    var baseEndpointUrl = this.Session.GetBaseEndpointUrl();

            //    if (!errorMessages.Any())
            //    {
            //        var monitoredItemsDict = this.Command.MonitoredItems.ToDictionary(monitoredItem => monitoredItem.NodeId);
            //        var newSubscriptionDto = new SubscriptionDto
            //        {
            //            EndpointUrl = baseEndpointUrl,
            //            MonitoredItems = monitoredItemsDict
            //        };

            //        if (!this._subscriptionRepository.Add(newSubscriptionDto))
            //            errorMessages.Add($"Bad: Could not create/update entry configuration for subscription.");
            //    }
            //}

            this.AddMessagesForAnyInvalidNodes(errorMessages);

            if (!errorMessages.Any())
                this.Logger.Debug($"Created/Updated alarm subscriptions on Endpoint: [{this.EndpointUrl}]");

            return this.GetStatusMessage(errorMessages);
        }

        private void AddMessagesForAnyInvalidNodes(ICollection<string> errorMessages)
        {
            if (!this._groupedItemsNotCreated.Any())
                return;

            var stringBuilder = new StringBuilder("Bad: Some nodes could not be subscribed to. ");
            foreach (var group in this._groupedItemsNotCreated)
            {
                stringBuilder.Append($"{@group.Key}: {string.Join(", ", @group.Value)} ");
            }

            errorMessages.Add(stringBuilder.ToString().TrimEnd());
        }

        private Action<AlarmSubscriptionMonitoredItem[]> SubscribeBatch()
        {
            return (monitoredItems) =>
            {
                Opc.Ua.Client.Subscription opcUaSubscription = default;
                foreach (var monitoredItem in monitoredItems)
                {
                    opcUaSubscription = this.GetSubscription(monitoredItem);

                    opcUaSubscription = opcUaSubscription == default
                        ? this.CreateNewSubscription(monitoredItem)
                        : this.ModifySubscription(opcUaSubscription, monitoredItem);
                }

                try
                {
                    opcUaSubscription?.ApplyChanges();
                }
                catch (ServiceResultException sre)
                {
                    this.Logger.Error(sre, $"Failed to call ApplyChanges() for batch with {monitoredItems.Length} items: ");
                    throw;
                }

                this.CollectInvalidNodes(monitoredItems, opcUaSubscription);
            };
        }

        private void CollectInvalidNodes(AlarmSubscriptionMonitoredItem[] items, Opc.Ua.Client.Subscription opcUaSubscription)
        {
            var nodeIds = new List<NodeId>();
            foreach (var monitoredItem in items)
                nodeIds.Add(new NodeId(monitoredItem.NodeId));

            var itemsNotCreatedGroups = opcUaSubscription?.MonitoredItems
                .Join(nodeIds, item => item.ResolvedNodeId.ToString(), item => item.ToString(), (item, monitoredItem) => item)
                .Where(item => !item.Created)
                .GroupBy(item => item.Status.Error.StatusCode);

            foreach (var itemsNotCreatedGroup in itemsNotCreatedGroups!)
            {
                var key = itemsNotCreatedGroup.Key.ToString();
                var nodes = itemsNotCreatedGroup.Select(item => item.StartNodeId.ToString()).ToList();

                var success = this._groupedItemsNotCreated.TryGetValue(key, out var list);
                if (success)
                    list.AddRange(nodes);
                else
                {
                    list = nodes;
                    this._groupedItemsNotCreated.Add(key, list);
                }
            }
        }

        private async Task<List<string>> AddValidationErrorsAsync()
        {
            var errorMessages = new List<string>();

            for (var itemIndex = 0; itemIndex < this.Command.AlarmMonitoredItems.Length; itemIndex++)
            {
                var results = await this._alarmMonitoredItemValidator.ValidateAsync(this.Command.AlarmMonitoredItems[itemIndex]);
                if (results.IsValid) continue;
                var validationMessages = results.ToString(";");
                errorMessages.Add($"Bad: {nameof(CreateAlarmSubscriptionsRequest.AlarmMonitoredItems)}[{itemIndex}]: {validationMessages}.");
            }
            return errorMessages;
        }

        protected override void GenerateResult(CreateAlarmSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.CreateAlarmSubscription;
            result.Message = message;
        }

        private Opc.Ua.Client.Subscription CreateNewSubscription(AlarmSubscriptionMonitoredItem monitoredItem)
        {
            var keepAliveCount = Convert.ToUInt32(monitoredItem.HeartbeatInterval);
            var subscription = this.Session.Subscriptions.FirstOrDefault(x => monitoredItem.PublishingInterval.Equals(x.PublishingInterval.ToString()));
            if (subscription == default)
            {
                subscription = new Opc.Ua.Client.Subscription
                {
                    PublishingInterval = int.Parse(monitoredItem.PublishingInterval),
                    LifetimeCount = 100000,
                    KeepAliveCount = keepAliveCount > 0 ? keepAliveCount : 100000,
                    MaxNotificationsPerPublish = 1000,
                    Priority = 0,
                    PublishingEnabled = false
                };
                this.Session.AddSubscription(subscription);
                subscription.Create();
            }
            var item = this.CreateMonitoredItem(monitoredItem);
            subscription.AddItem(item);
            return subscription;
        }

        private Opc.Ua.Client.Subscription ModifySubscription(
            Opc.Ua.Client.Subscription opcUaSubscription, AlarmSubscriptionMonitoredItem monitoredItem)
        {
            var existingItems = opcUaSubscription
                .MonitoredItems
                .Where(x => monitoredItem.NodeId.Equals(x.ResolvedNodeId.ToString()))
                .ToList();

            //TODO: Implement logic to detect a change in subscription, e.g. filter

            opcUaSubscription.RemoveItems(existingItems);// Notification of intent
            opcUaSubscription.ApplyChanges(); // enforces intent is executed
            return this.CreateNewSubscription(monitoredItem); // now re-add the monitored item
        }

        private MonitoredItem CreateMonitoredItem(AlarmSubscriptionMonitoredItem alarmSubscriptionMonitoredItem)
        {
            MonitoredItem monitoredItem = null;
            try
            {
                monitoredItem = InitializeAlarmMonitoredItem(alarmSubscriptionMonitoredItem, this.ComplexTypeSystem, this._messageMetadata, this.Session);

                this.Logger.Trace($"Alarm monitored item with NodeId: [{alarmSubscriptionMonitoredItem.NodeId}] " +
                                        $", Sampling Interval: [{monitoredItem.SamplingInterval}] and " +
                                        $"Publishing Interval: [{alarmSubscriptionMonitoredItem.PublishingInterval}] " +
                                        "has been created successfully");
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning($"Unable to create alarm monitored item with NodeId: [{alarmSubscriptionMonitoredItem.NodeId}]", ex);
            }

            return monitoredItem;
        }

        private MonitoredItem InitializeAlarmMonitoredItem(AlarmSubscriptionMonitoredItem monitoredItem, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata telemetryMessageMetadata, Session session)
        {
            this._opcAlarmMonitoredItemService.Initialize(monitoredItem, complexTypeSystem, telemetryMessageMetadata, session);
            return this._opcAlarmMonitoredItemService as MonitoredItem;
        }
    }
}
