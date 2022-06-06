// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
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
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;
using OMP.Connector.Domain.Schema.Responses.AlarmSubscription;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.AlarmSubscription
{
    public delegate MonitoredItem AlarmMonitoredItemServiceInitializerFactoryDelegate(
        AlarmSubscriptionMonitoredItem monitoredItem,
        IComplexTypeSystem complexTypeSystem,
        TelemetryMessageMetadata telemetryMessageMetaData,
        Session session);

    public class CreateAlarmSubscriptionProvider : AlarmSubscriptionProvider<CreateAlarmSubscriptionsRequest, CreateAlarmSubscriptionsResponse>
    {
        private readonly IAlarmSubscriptionRepository _subscriptionRepository;
        private readonly TelemetryMessageMetadata _messageMetadata;
        private readonly int _batchSize;
        private readonly AlarmMonitoredItemServiceInitializerFactoryDelegate _opcMonitoredItemServiceInitializerFactory;
        private readonly Dictionary<string, List<string>> _groupedItemsNotCreated;
        private FilterDefinition m_filter;

        public CreateAlarmSubscriptionProvider(
            IAlarmSubscriptionRepository subscriptionRepository,
            ILogger<CreateAlarmSubscriptionProvider> logger,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            AlarmMonitoredItemServiceInitializerFactoryDelegate opcMonitoredItemServiceInitializerFactory,
            CreateAlarmSubscriptionsRequest command,
            TelemetryMessageMetadata messageMetadata,
            MonitoredItemValidator monitoredItemValidator) : base(command, connectorConfiguration, logger)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._opcMonitoredItemServiceInitializerFactory = opcMonitoredItemServiceInitializerFactory;
            this._messageMetadata = messageMetadata;
            this._batchSize = this.Settings.OpcUa.SubscriptionBatchSize;

            this._groupedItemsNotCreated = new Dictionary<string, List<string>>();
        }

        protected override async Task<string> ExecuteCommandAsync()
        {
            // create the default subscription.
            var m_subscription = new Opc.Ua.Client.Subscription();

            m_subscription.DisplayName = null;
            m_subscription.PublishingInterval = 1000;
            m_subscription.KeepAliveCount = 10;
            m_subscription.LifetimeCount = 100;
            m_subscription.MaxNotificationsPerPublish = 1000;
            m_subscription.PublishingEnabled = true;
            m_subscription.TimestampsToReturn = TimestampsToReturn.Both;

            Session.AddSubscription(m_subscription);
            m_subscription.Create();

            foreach (var command in Command.MonitoredItems)
            {
                m_subscription.AddItem(CreateMonitoredItem(command));
            }

            m_subscription.ApplyChanges();

            this.Logger.Debug($"Created/Updated alarm subscriptions on Endpoint: [{this.EndpointUrl}]");

            return this.GetStatusMessage(new List<string>());
        }

        protected override void GenerateResult(CreateAlarmSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.CreateAlarmSubscription;
            result.Message = message;
        }

        private MonitoredItem CreateMonitoredItem(AlarmSubscriptionMonitoredItem subscriptionMonitoredItem)
        {
            MonitoredItem monitoredItem = null;
            try
            {
                monitoredItem = this._opcMonitoredItemServiceInitializerFactory.Invoke(subscriptionMonitoredItem,
                    this.ComplexTypeSystem, this._messageMetadata, this.Session);

                this.Logger.Trace($"Alarm monitored item with NodeId: [{subscriptionMonitoredItem.NodeId}] " +
                                        $", Sampling Interval: [{monitoredItem.SamplingInterval}] and " +
                                        $"Publishing Interval: [{subscriptionMonitoredItem.PublishingInterval}] " +
                                        "has been created successfully");
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning($"Unable to create monitored item with NodeId: [{subscriptionMonitoredItem.NodeId}]", ex);
            }

            return monitoredItem;
        }
    }
}
