// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using OMP.PlantConnectivity.OpcUA.Configuration;
using OMP.PlantConnectivity.OpcUA.Repositories;
using OMP.PlantConnectivity.OpcUA.Sessions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;
using CreateAlarmSubscriptionResponse = OMP.PlantConnectivity.OpcUA.Models.Alarms.CreateAlarmSubscriptionResponse;
using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Services.Subscriptions;

namespace OMP.PlantConnectivity.OpcUA.Services.Alarms
{
    internal sealed class AlarmSubscriptionCommandService : IAlarmSubscriptionCommandService
    {
        private const bool DoNotAutoApplyChangesOnCreatedOrModify = false;
        private readonly IValidator<AlarmSubscriptionMonitoredItem> alarmMonitoredItemValidator;
        private readonly IAlarmSubscriptionRepository? alarmSubscriptionRepository;
        private readonly IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors;
        private readonly ILogger<AlarmSubscriptionCommandService> logger;
        private readonly OmpOpcUaConfiguration opcUaConfiguration;

        public AlarmSubscriptionCommandService(
            IOptions<OmpOpcUaConfiguration> options,
            IValidator<AlarmSubscriptionMonitoredItem> alarmMonitoredItemValidator,
            IAlarmSubscriptionRepository? alarmSubscriptionRepository,
            IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors,
            ILogger<AlarmSubscriptionCommandService> logger)
        {
            this.alarmMonitoredItemValidator = alarmMonitoredItemValidator;
            this.alarmSubscriptionRepository = alarmSubscriptionRepository;
            this.monitoredItemMessageProcessors = monitoredItemMessageProcessors;
            this.logger = logger;
            opcUaConfiguration = options.Value;
        }

        public async Task<CreateAlarmSubscriptionResponse> CreateAlarmSubscriptions(IOpcUaSession opcUaSession, CreateAlarmSubscriptionsCommand command, CancellationToken CancellationToken)
        {
            EnsureAlarmMonitoredItemMessageProcessorsExist();

            var errorMessages = await AddValidationErrorsAsync(command);
            if (errorMessages.Any())
            {
                logger.LogDebug(
                    "Validation of {monitoredItems} was not successful. Errors: {errors}"
                    , nameof(CreateAlarmSubscriptionsCommand.AlarmMonitoredItems), string.Join(" | ", errorMessages));

                var errorResults = new CreateAlarmSubscriptionResult(errorMessages);
                var errorResponse = new CreateAlarmSubscriptionResponse(command, errorResults, false);
                return errorResponse;
            }

            var globalResults = new List<AlarmMonitoredItemResult>();

            try
            {
                var alarmMonitoredItemGroups = command.AlarmMonitoredItems.GroupBy(item => item.PublishingInterval);

                foreach (var group in alarmMonitoredItemGroups)
                {
                    var groupItems = group.ToList();
                    var batchHandler = new BatchHandler<AlarmSubscriptionMonitoredItem>(
                        opcUaConfiguration.AlarmSubscriptionBatchSize,
                        SubscribeInBatchAndInsertResults(opcUaSession, globalResults));

                    batchHandler.RunBatches(groupItems);
                    logger.LogTrace("Alarm subscription with publishing interval {publishingInterval} ms: Subscribed to {nodes} nodes.", group.Key, groupItems.Count);
                }

                opcUaSession.ActivatePublishingOnAllSubscriptions();
                opcUaSession.RefreshAlarmsOnAllSubscriptions();
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                var errorResults = new CreateAlarmSubscriptionResult(errorMessages);
                return new CreateAlarmSubscriptionResponse(
                       command,
                       errorResults,
                       false,
                        $"Could not create / update alarm subscriptions. {error.Message}");
            }

            var baseEndpointUrl = opcUaSession.GetBaseEndpointUrl();
            alarmSubscriptionRepository?.Add(baseEndpointUrl, command.AlarmMonitoredItems);

            var allSucceeded = globalResults.All(r => r.StatusCode == StatusCodes.Good);
            if (allSucceeded)
                logger.LogDebug("Created/updated alarm subscriptions on Endpoint: [{endpointUrl}]", command.EndpointUrl);
            else
                logger.LogError("Errors occred during alarm subscriptions create/update on Endpoint: [{endpointUrl}]", command.EndpointUrl);

            var results = new CreateAlarmSubscriptionResult(globalResults);
            var response = new CreateAlarmSubscriptionResponse(command, results, allSucceeded);
            return response;
        }

        public async Task<RemoveAllAlarmSubscriptionsResponse> RemoveAllAlarmSubscriptions(IOpcUaSession opcUaSession, RemoveAllAlarmSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            var response = new RemoveAllAlarmSubscriptionsResponse
            {
                Succeeded = true,
                Command = command
            };

            try
            {
                RemoveAllAlarmSubscriptionsFromRepository(command.EndpointUrl);

                var activeSubscriptions = opcUaSession.GetSubscriptions().ToArray();
                foreach (var subscription in activeSubscriptions)
                {
                    subscription.SetPublishingMode(false);
                    logger.LogTrace("Disabled publishing for alarm subscription [Id: {subscriptionId}]", subscription.Id);
                }

                await opcUaSession.RemoveSubscriptionsAsync(activeSubscriptions);
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                response.Succeeded = false;
                response.Message = error.Message;
                logger.LogWarning(error, "Unable to remove all alarm subscriptions from OPC UA server session: {error}", error.Message);
            }

            return response;
        }

        public async Task<RemoveAlarmSubscriptionsResponse> RemoveAlarmSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveAlarmSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            alarmSubscriptionRepository?.DeleteAlarmMonitoredItems(command.EndpointUrl, command.NodeIds);

            var response = await RemoveAlarmSubscriptionsFromSessionAsync(opcUaSession, command);
            if (!response.Succeeded)
            {
                logger.LogError("Could not remove alarm subscriptions from OPC UA session on Endpoint: [{endpointUrl}]", command.EndpointUrl);
                response.Message = "Could not remove alarm subscriptions from OPC UA session.";
            }
            else
            {
                logger.LogDebug("Removed monitored items from alarm subscription(s) on Endpoint: [{endpointUrl}]", command.EndpointUrl);
            }

            return response;
        }

        #region [Privates]
        private async Task<List<AlarmMonitoredItemResult>> AddValidationErrorsAsync(CreateAlarmSubscriptionsCommand command)
        {
            var errorMessages = new List<AlarmMonitoredItemResult>();
            for (var itemIndex = 0; itemIndex < command.AlarmMonitoredItems.Count; itemIndex++)
            {
                var monitoredItem = command.AlarmMonitoredItems[itemIndex];
                var results = await alarmMonitoredItemValidator.ValidateAsync(monitoredItem);
                if (results.IsValid)
                    continue;

                var validationMessages = $"{nameof(CreateSubscriptionsCommand.MonitoredItems)}[{itemIndex}]: {results.ToString(";")}.";
                errorMessages.Add(new AlarmMonitoredItemResult(monitoredItem, StatusCodes.Bad, validationMessages));
            }
            return errorMessages;
        }

        private Action<AlarmSubscriptionMonitoredItem[]> SubscribeInBatchAndInsertResults(IOpcUaSession opcUaSession, List<AlarmMonitoredItemResult> results)
        {
            return (monitoredItems) =>
            {
                var subscriptionList = new HashSet<Subscription>();
                foreach (var monitoredItem in monitoredItems)
                {
                    var subscription = opcUaSession.CreateOrUpdateAlarmSubscription(monitoredItem, DoNotAutoApplyChangesOnCreatedOrModify);
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

        private static IEnumerable<AlarmMonitoredItemResult> GetResults(AlarmSubscriptionMonitoredItem[] items, Subscription subscription)
        {
            var allMonotoredItemsInterestedIn = subscription.MonitoredItems
                                                            .Where(mi => items.Any(i => i.NodeId == mi.StartNodeId))
                                                            .Zip(items);
            return allMonotoredItemsInterestedIn
                .Select(mi => GetAlarmMonitoredItemResult(mi.Second, mi.First))
                .ToArray();
        }

        private static AlarmMonitoredItemResult GetAlarmMonitoredItemResult(AlarmSubscriptionMonitoredItem item, MonitoredItem monitoredItem)
        {
            if (monitoredItem.Status.Error is null)
            {
                if (monitoredItem.Created)
                    return new AlarmMonitoredItemResult(item, StatusCodes.Good, nameof(StatusCodes.Good));

                return new AlarmMonitoredItemResult(item, StatusCodes.Uncertain, $"{StatusCodes.Uncertain} of status but no error was detected");
            }


            return new AlarmMonitoredItemResult(item, monitoredItem.Status.Error.StatusCode, monitoredItem.Status.Error.AdditionalInfo);
        }
        private void EnsureAlarmMonitoredItemMessageProcessorsExist()
        {
            if (!monitoredItemMessageProcessors.Any())
            {
                logger.LogError("{CreateAlarmSubscriptionsMethod} is not supported: No {AlarmMonitoredItemMessageProcessor} found.", nameof(CreateAlarmSubscriptions), nameof(IAlarmMonitoredItemMessageProcessor));
                throw new NotSupportedException($"{nameof(CreateAlarmSubscriptions)} is not supported: No {nameof(IAlarmMonitoredItemMessageProcessor)} found.");
            }
        }


        private async Task<RemoveAlarmSubscriptionsResponse> RemoveAlarmSubscriptionsFromSessionAsync(IOpcUaSession opcUaSession, RemoveAlarmSubscriptionsCommand command)
        {
            var response = new RemoveAlarmSubscriptionsResponse
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
                    logger.LogDebug("{items} monitored items were removed from alarm subscription [Id: {subscriptionId}]",
                        items.Count, subscription.Id);

                    if (!subscription.MonitoredItems.Any())
                        await opcUaSession.RemoveSubscriptionAsync(subscription);

                    response.NodesWithActiveAlarmSubscriptions.AddRange(subscription.MonitoredItems.Select(mi => mi.StartNodeId));
                }
            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                logger.LogWarning(ex, $"Unable to remove alarm subscriptions from OPC UA server session");
            }
            return response;
        }

        private List<(Subscription, List<MonitoredItem>)> GetActiveMonitoredItems(IOpcUaSession opcUaSession, RemoveAlarmSubscriptionsCommand command)
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

        private Action<MonitoredItem[]> UnsubscribeBatches(Subscription subscription, RemoveAlarmSubscriptionsResponse removeAlarmSubscriptionsResponse)
        {
            return monitoredItems =>
            {
                try
                {
                    subscription.RemoveItems(monitoredItems);
                    subscription.ApplyChanges();
                    removeAlarmSubscriptionsResponse.NodesRemovedFromAlarmSubscriptions.AddRange(monitoredItems.Select(mi => mi.StartNodeId));
                }
                catch (ServiceResultException sre)
                {
                    logger.LogError(sre, "Failed to call ApplyChanges() for batch with {monitoredItems} items ", monitoredItems.Count());
                    throw;
                }
            };
        }

        private bool RemoveAllAlarmSubscriptionsFromRepository(string endpointUrl)
        {
            if (alarmSubscriptionRepository is null)
                return true;

            var isSuccess = true;
            try
            {
                var subscriptions = alarmSubscriptionRepository.GetAllByEndpointUrl(endpointUrl);
                if (subscriptions is null || !subscriptions.Any())
                    return true;

                foreach (var subscription in subscriptions)
                    isSuccess = alarmSubscriptionRepository.Remove(subscription) && isSuccess;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.LogWarning(ex, "Unable to remove all alarm subscriptions from the subscription repository.");
            }

            return isSuccess;
        }
        #endregion
    }
}
