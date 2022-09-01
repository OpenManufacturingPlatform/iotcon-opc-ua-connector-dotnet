// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using ApplicationV2.Configuration;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Repositories;
using ApplicationV2.Sessions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2.Services
{
    public interface IMonitoredItemMessageProcessor
    {
        string Identifier { get; }
        void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments);
    }

    public class LoggingMonitoredItemMessageProcessor : IMonitoredItemMessageProcessor //This is the default OMP implimentations
    {
        private readonly ILogger<LoggingMonitoredItemMessageProcessor> logger;

        public LoggingMonitoredItemMessageProcessor(ILogger<LoggingMonitoredItemMessageProcessor> logger)
        {
            this.logger = logger;
        }

        public string Identifier { get; } = nameof(LoggingMonitoredItemMessageProcessor);
        public void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments)
        {
            logger.LogInformation("Message Received: NodeId: {nodeId} | Value: {value}", monitoredItem.StartNodeId, eventArguments.NotificationValue);
        }
    }

    public interface ISubscriptionCommandService
    {
        Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<RemoveSubscriptionsResponse> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken);
        Task<RemoveAllSubscriptionsResponse> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
    }

    internal class SubscriptionCommandService : ISubscriptionCommandService
    {
        private const bool DoNotAutoApplyChangesOnCreatedOrModify = false;
        private readonly IValidator<SubscriptionMonitoredItem> monitoredItemValidator;
        private readonly ISubscriptionRepository? subscriptionRepository;
        private readonly IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors;
        private readonly ILogger<SubscriptionCommandService> logger;
        private readonly OmpOpcUaConfiguration opcUaConfiguration;
        protected readonly ConnectorConfiguration connectorConfiguration;

        public SubscriptionCommandService(
            IOptions<ConnectorConfiguration> options,
            IValidator<SubscriptionMonitoredItem> monitoredItemValidator,
            ISubscriptionRepository? subscriptionRepository,
            IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors,
            ILogger<SubscriptionCommandService> logger)
        {
            this.monitoredItemValidator = monitoredItemValidator;
            this.subscriptionRepository = subscriptionRepository;
            this.monitoredItemMessageProcessors = monitoredItemMessageProcessors;
            this.logger = logger;
            this.connectorConfiguration = options.Value;
            this.opcUaConfiguration = options.Value.OpcUa;

            ValidateMonitoredItemMessageProcessors();//TODO: Discuss enhancement [Currently this will make entire app crash]
        }
        public async Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken)
        {
            var errorMessages = await this.AddValidationErrorsAsync(command);
            if (errorMessages.Any())
            {
                logger.LogDebug(
                    "Validation of {monitoredItems} was not successful. Errors: {errors}"
                    , nameof(CreateSubscriptionsCommand.MonitoredItems), string.Join(" | ", errorMessages));

                var results = new CreateSubscriptionResult(errorMessages);
                var responses = new CreateSubscriptionResponse(command, results, false);
                return responses;
            }

            var globalResults = new List<MonitoriedItemResult>();

            try
            {
                var monitoredItemGroups = command.MonitoredItems.GroupBy(item => item.PublishingInterval);

                foreach (var group in monitoredItemGroups)
                {
                    var groupItems = group.ToList();
                    var batchHandler = new BatchHandler<SubscriptionMonitoredItem>(
                        opcUaConfiguration.SubscriptionBatchSize,
                        SubscribeInBatchAndInsertResults(opcUaSession, globalResults));

                    batchHandler.RunBatches(groupItems);
                    logger.LogTrace("Subscription with publishing interval {publishingInterval} ms: Subscribed to {nodes} nodes.", group.Key, groupItems.Count);
                }

                opcUaSession.ActivatePublishingOnAllSubscriptions();
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                var results = new CreateSubscriptionResult(errorMessages);
                return new CreateSubscriptionResponse(
                       command,
                       results,
                       false,
                        $"Could not create / update subscriptions. {error.Message}");
            }


            var baseEndpointUrl = opcUaSession.GetBaseEndpointUrl();
            subscriptionRepository?.Add(baseEndpointUrl, command.MonitoredItems);

            var allSucceeded = globalResults.All(r => r.StatusCode == StatusCodes.Good);
            if (allSucceeded)
                logger.LogDebug("Created/Updated subscriptions on Endpoint: [{endpointUrl}]", command.EndpointUrl);
            else
                logger.LogError("Errors occred during subscriptions Create/update on Endpoint: [{endpointUrl}]", command.EndpointUrl);

            var resulst = new CreateSubscriptionResult(globalResults);
            var response = new CreateSubscriptionResponse(command, resulst, allSucceeded);
            return response;
        }

        public async Task<RemoveAllSubscriptionsResponse> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            var response = new RemoveAllSubscriptionsResponse
            {
                Succeeded = true,
                Command = command
            };

            try
            {
                RemoveAllSubscriptionsFromRepository(command.EndpointUrl);

                var activeSubscriptions = opcUaSession.GetSubscriptions().ToArray();
                foreach (var subscription in activeSubscriptions)
                {
                    subscription.SetPublishingMode(false);
                    logger.LogTrace("Disabled publishing for subscription [Id: {subscriptionId}]", subscription.Id);
                }

                await opcUaSession.RemoveSubscriptionsAsync(activeSubscriptions);
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                response.Succeeded = false;
                response.Message = error.Message;
                logger.LogWarning(error, "Unable to remove all subscriptions from OPC UA server session: {error}", error.Message);
            }

            return response;
        }

        public async Task<RemoveSubscriptionsResponse> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            subscriptionRepository?.DeleteMonitoredItems(command.EndpointUrl, command.NodeIds);

            var response = await RemoveSubscriptionsFromSessionAsync(opcUaSession, command);
            if (!response.Succeeded)
            {
                logger.LogError("Could not remove subscriptions from OPC UA session on Endpoint: [{endpointUrl}]", command.EndpointUrl);
                response.Message = "Could not remove subscriptions from OPC UA session.";
            }
            else
            {
                logger.LogDebug("Removed monitored items from subscription(s) on Endpoint: [{endpointUrl}]", command.EndpointUrl);
            }

            return response;
        }

        #region [Privates]
        private async Task<List<MonitoriedItemResult>> AddValidationErrorsAsync(CreateSubscriptionsCommand command)
        {
            var errorMessages = new List<MonitoriedItemResult>();
            for (var itemIndex = 0; itemIndex < command.MonitoredItems.Count; itemIndex++)
            {
                var monitoredItem = command.MonitoredItems[itemIndex];
                var results = await monitoredItemValidator.ValidateAsync(monitoredItem);
                if (results.IsValid)
                    continue;

                var validationMessages = $"{nameof(CreateSubscriptionsCommand.MonitoredItems)}[{itemIndex}]: {results.ToString(";")}.";
                errorMessages.Add(new MonitoriedItemResult(monitoredItem, StatusCodes.Bad, validationMessages));
            }
            return errorMessages;
        }

        private Action<SubscriptionMonitoredItem[]> SubscribeInBatchAndInsertResults(IOpcUaSession opcUaSession, List<MonitoriedItemResult> results)
        {
            return (monitoredItems) =>
            {
                var subscriptionList = new HashSet<Subscription>();
                foreach (var monitoredItem in monitoredItems)
                {
                    var subscription = opcUaSession.CreateOrUpdateSubscription(monitoredItem, DoNotAutoApplyChangesOnCreatedOrModify);
                    subscriptionList.Add(subscription);
                }

                try
                {
                    foreach (var subscription in subscriptionList)
                    {
                        subscription.ApplyChanges();
                        var localResults = GetResults(monitoredItems, subscription);
                        results.AddRange(localResults);
                    }
                }
                catch (ServiceResultException sre)
                {
                    logger.LogError(sre, "Failed to call ApplyChanges() for batch with {monitoredItems} items: ", monitoredItems.Count());
                    throw;
                }
            };
        }

        private static IEnumerable<MonitoriedItemResult> GetResults(SubscriptionMonitoredItem[] items, Subscription subscription)
        {
            var allMonotoredItemsInterestedIn = subscription.MonitoredItems
                                                            .Where(mi => items.Any(i => i.NodeId == mi.StartNodeId))
                                                            .Zip(items);
            return allMonotoredItemsInterestedIn
                .Select(mi => GetMonitoriedItemResult(mi.Second, mi.First))
                .ToArray();
        }

        private static MonitoriedItemResult GetMonitoriedItemResult(SubscriptionMonitoredItem item, MonitoredItem monitoredItem)
        {
            if (monitoredItem.Status.Error is null)
            {
                if (monitoredItem.Created)
                    return new MonitoriedItemResult(item, StatusCodes.Good, nameof(StatusCodes.Good));

                return new MonitoriedItemResult(item, StatusCodes.Uncertain, $"{StatusCodes.Uncertain} of status but no error was detected");
            }


            return new MonitoriedItemResult(item, monitoredItem.Status.Error.StatusCode, monitoredItem.Status.Error.AdditionalInfo);
        }
        private void ValidateMonitoredItemMessageProcessors()
        {
            if (!monitoredItemMessageProcessors.Any())
            {
                logger.LogError("No {MonitoredItemMessageProcessor} found", nameof(IMonitoredItemMessageProcessor));
                throw new ArgumentException(nameof(monitoredItemMessageProcessors));
            }
        }


        private async Task<RemoveSubscriptionsResponse> RemoveSubscriptionsFromSessionAsync(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command)
        {
            var response = new RemoveSubscriptionsResponse
            {
                Succeeded = true
            };

            try
            {
                var activeSubscriptions = GetActiveMonitoredItems(opcUaSession, command);
                var activeSubscription = opcUaSession.GetSubscriptions();


                foreach (var (subscription, items) in activeSubscriptions)
                {
                    var batchHandler = new BatchHandler<MonitoredItem>(opcUaConfiguration.SubscriptionBatchSize, UnsubscribeBatches(subscription, response));
                    batchHandler.RunBatches(items);
                    logger.LogDebug("{items} monitored items were removed from subscription [Id: {subscriptionId}]",
                        items.Count, subscription.Id);

                    if (!subscription.MonitoredItems.Any())
                        await opcUaSession.RemoveSubscriptionAsync(subscription);

                    response.NodesWithActiveSubscriptions.AddRange(subscription.MonitoredItems.Select(mi => mi.StartNodeId));
                }
            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                logger.LogWarning(ex, $"Unable to remove subscriptions from OPC UA server session");
            }
            return response;
        }

        private List<(Subscription, List<MonitoredItem>)> GetActiveMonitoredItems(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command)
        {
            var nodeIds = new List<NodeId>();
            foreach (var nodeId in command.NodeIds)
                nodeIds.Add(new NodeId(nodeId));

            return (from subscription in opcUaSession.GetSubscriptions()
                    select (subscription, (from nodeId in nodeIds
                                           join monitoredItem in subscription.MonitoredItems
                                               on nodeId.ToString() equals monitoredItem.ResolvedNodeId.ToString()
                                           select monitoredItem).ToList()
                        )).ToList();
        }

        private Action<MonitoredItem[]> UnsubscribeBatches(Subscription subscription, RemoveSubscriptionsResponse removeSubscriptionsResponse)
        {
            return monitoredItems =>
            {
                try
                {
                    subscription.RemoveItems(monitoredItems);
                    subscription.ApplyChanges();
                    removeSubscriptionsResponse.NodesOfRemovedSubscriptions.AddRange(monitoredItems.Select(mi => mi.StartNodeId));
                }
                catch (ServiceResultException sre)
                {
                    logger.LogError(sre, "Failed to call ApplyChanges() for batch with {monitoredItems} items ", monitoredItems.Count());
                    throw;
                }
            };
        }

        private bool RemoveAllSubscriptionsFromRepository(string endpointUrl)
        {
            if (subscriptionRepository is null)
                return true;

            var isSuccess = true;
            try
            {
                var subscriptions = subscriptionRepository.GetAllByEndpointUrl(endpointUrl);
                if (subscriptions is null || !subscriptions.Any())
                    return true;

                foreach (var subscription in subscriptions)
                    isSuccess = subscriptionRepository.Remove(subscription) && isSuccess;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.LogWarning(ex, "Unable to remove all subscriptions from the subscription repository.");
            }

            return isSuccess;
        }

        #endregion
    }
}
