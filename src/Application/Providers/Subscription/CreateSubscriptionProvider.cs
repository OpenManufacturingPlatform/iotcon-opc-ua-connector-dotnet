using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Application.Providers.Subscription.Base;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.Subscription;
using OMP.Connector.Domain.Schema.Responses.Subscription;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.Subscription
{
    public delegate MonitoredItem MonitoredItemServiceInitializerFactoryDelegate(
        SubscriptionMonitoredItem monitoredItem,
        IComplexTypeSystem complexTypeSystem,
        TelemetryMessageMetadata telemetryMessageMetaData);

    public class CreateSubscriptionProvider : SubscriptionProvider<CreateSubscriptionsRequest, CreateSubscriptionsResponse>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IEndpointDescriptionRepository _endpointDescriptionRepository;
        private readonly TelemetryMessageMetadata _messageMetadata;
        private readonly MonitoredItemValidator _monitoredItemValidator;
        private readonly int _batchSize;
        private readonly MonitoredItemServiceInitializerFactoryDelegate _opcMonitoredItemServiceInitializerFactory;
        private readonly Dictionary<string, List<string>> _groupedItemsNotCreated;

        public CreateSubscriptionProvider(
            ISubscriptionRepository subscriptionRepository,
            ILogger<CreateSubscriptionProvider> logger,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            MonitoredItemServiceInitializerFactoryDelegate opcMonitoredItemServiceInitializerFactory,
            CreateSubscriptionsRequest command,
            TelemetryMessageMetadata messageMetadata,
            MonitoredItemValidator monitoredItemValidator,
            IEndpointDescriptionRepository endpointDescriptionRepository) : base(command, connectorConfiguration, logger)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._opcMonitoredItemServiceInitializerFactory = opcMonitoredItemServiceInitializerFactory;
            this._messageMetadata = messageMetadata;
            this._monitoredItemValidator = monitoredItemValidator;
            this._endpointDescriptionRepository = endpointDescriptionRepository;
            this._batchSize = this.Settings.OpcUa.SubscriptionBatchSize;

            this._groupedItemsNotCreated = new Dictionary<string, List<string>>();
        }

        protected override async Task<string> ExecuteCommandAsync()
        {
            var errorMessages = await this.AddValidationErrorsAsync();
            if (errorMessages.Any())
            {
                this.Logger.Debug($"Validation of {nameof(CreateSubscriptionsRequest.MonitoredItems)} was not successful.");
                this.Logger.Trace(string.Join(" ", errorMessages));
                return this.GetStatusMessage(errorMessages);
            }

            try
            {
                var subscriptionGroups = this.Command.MonitoredItems.GroupBy(item => item.PublishingInterval);

                foreach (var group in subscriptionGroups)
                {
                    var groupItems = group.ToList();
                    var batchHandler = new BatchHandler<SubscriptionMonitoredItem>(this._batchSize, this.SubscribeBatch());
                    batchHandler.RunBatches(groupItems);
                    this.Logger.Trace($"Subscription with publishing interval {group.Key} ms: Subscribed to {groupItems.Count} nodes.");
                }

                foreach (var sub in this.Session.Subscriptions.Where(sub => !sub.PublishingEnabled))
                {
                    this.Logger.Trace($"Enabling publishing for subscription {sub.Id}");
                    sub.SetPublishingMode(true);
                }
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                errorMessages.Add($"Bad: Could not create / update subscriptions. {error.Message}");
            }

            if (!base.Settings.DisableSubscriptionRestoreService && !errorMessages.Any())
            {
                var baseEndpointUrl = this.Session.GetBaseEndpointUrl();
                var endpoint = this._endpointDescriptionRepository.GetByEndpointUrl(baseEndpointUrl);

                if (endpoint == null)
                {
                    var emptyEndpoint = new EndpointDescriptionDto
                    {
                        EndpointUrl = baseEndpointUrl,
                        ServerDetails = new ServerDetails
                        {
                            Name = string.Empty,
                            Route = string.Empty
                        }
                    };
                    if (!this._endpointDescriptionRepository.Add(emptyEndpoint))
                        errorMessages.Add($"Bad: Could not create/update entry configuration for endpoint.");
                }
                if (!errorMessages.Any())
                {
                    var monitoredItemsDict = this.Command.MonitoredItems.ToDictionary(monitoredItem => monitoredItem.NodeId);                    
                    var newSubscriptionDto = new SubscriptionDto
                    {
                        EndpointUrl = baseEndpointUrl,
                        MonitoredItems = monitoredItemsDict
                    };

                    if (!this._subscriptionRepository.Add(newSubscriptionDto))
                        errorMessages.Add($"Bad: Could not create/update entry configuration for subscription.");
                }
            }

            this.AddMessagesForAnyInvalidNodes(errorMessages);

            if (!errorMessages.Any())
                 this.Logger.Debug($"Created/Updated subscriptions on Endpoint: [{this.EndpointUrl}]");

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

        private Action<SubscriptionMonitoredItem[]> SubscribeBatch()
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
                    this.Logger.Error(sre, $"Failed to call ApplyChanges() for batch with {monitoredItems.Count()} items: ");
                    throw;
                }

                this.CollectInvalidNodes(monitoredItems, opcUaSubscription);
            };
        }

        private void CollectInvalidNodes(SubscriptionMonitoredItem[] items, Opc.Ua.Client.Subscription opcUaSubscription)
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

            for (var itemIndex = 0; itemIndex < this.Command.MonitoredItems.Length; itemIndex++)
            {
                var results = await this._monitoredItemValidator.ValidateAsync(this.Command.MonitoredItems[itemIndex]);
                if (results.IsValid) continue;
                var validationMessages = results.ToString(";");
                errorMessages.Add($"Bad: {nameof(CreateSubscriptionsRequest.MonitoredItems)}[{itemIndex}]: {validationMessages}.");
            }
            return errorMessages;
        }

        protected override void GenerateResult(CreateSubscriptionsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.CreateSubscription;
            result.Message = message;
        }

        private Opc.Ua.Client.Subscription CreateNewSubscription(SubscriptionMonitoredItem monitoredItem)
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
                    MaxNotificationsPerPublish = 1,
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
            Opc.Ua.Client.Subscription opcUaSubscription, SubscriptionMonitoredItem monitoredItem)
        {
            var existingItems = opcUaSubscription
                .MonitoredItems
                .Where(x => monitoredItem.NodeId.Equals(x.ResolvedNodeId.ToString()))
                .ToList();

            if (SamplingIntervalsAreTheSame(monitoredItem, existingItems)) 
                return opcUaSubscription;

            opcUaSubscription.RemoveItems(existingItems);// Notification of intent
            opcUaSubscription.ApplyChanges(); // enforces intent is executed
            return this.CreateNewSubscription(monitoredItem); // now re-add the monitored item
        }

        private static bool SamplingIntervalsAreTheSame(SubscriptionMonitoredItem monitoredItem, List<MonitoredItem> existingItems)
            => existingItems.Any(m => m.SamplingInterval == int.Parse(monitoredItem.SamplingInterval));

        private MonitoredItem CreateMonitoredItem(SubscriptionMonitoredItem subscriptionMonitoredItem)
        {
            MonitoredItem monitoredItem = null;
            try
            {
                monitoredItem = this._opcMonitoredItemServiceInitializerFactory.Invoke(subscriptionMonitoredItem,
                    this.ComplexTypeSystem, this._messageMetadata);

                this.Logger.Trace($"Monitored item with NodeId: [{subscriptionMonitoredItem.NodeId}] " +
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