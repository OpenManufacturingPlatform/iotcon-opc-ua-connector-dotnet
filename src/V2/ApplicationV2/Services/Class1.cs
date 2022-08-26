// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models;
using ApplicationV2.Sessions;
using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApplicationV2.Configuration;
using Opc.Ua;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2.Services
{
    public interface ISubscriptionCommandsService
    {
        Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<CommandResultBase> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken);

        Task<CommandResultBase> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
    }

    internal class SubscriptionCommandsService : ISubscriptionCommandsService
    {
        static Opc.Ua.StatusCode Bad = new Opc.Ua.StatusCode(0x80000000);
        private readonly IValidator<SubscriptionMonitoredItem> monitoredItemValidator;
        private readonly ILogger<SubscriptionCommandsService> logger;
        private readonly OpcUaConfiguration opcUaConfiguration;
        protected readonly ConnectorConfiguration connectorConfiguration;        

        public SubscriptionCommandsService(
            IOptions<ConnectorConfiguration> options,
            IValidator<SubscriptionMonitoredItem> monitoredItemValidator,
            ILogger<SubscriptionCommandsService> logger)
        {
            this.monitoredItemValidator = monitoredItemValidator;
            this.logger = logger;
            this.connectorConfiguration = options.Value;
            this.opcUaConfiguration = options.Value.OpcUa;
        }
        public async Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken)
        {
            var errorMessages = await this.AddValidationErrorsAsync(command);
            if (errorMessages.Any())
            {
                logger.LogDebug("Validation of {monitoredItems} was not successful. Errors: {errors}", nameof(CreateSubscriptionsCommand.MonitoredItems), string.Join(" | ", errorMessages));

                //CreateSubscriptionResult
                var errors = GetStatusMessage(errorMessages);
                var resulst = new CreateSubscriptionResult(
                    command.MonitoredItems.Select(n => n.NodeId),
                    errorMessages.Select(e => new Opc.Ua.StatusCode()));

                var response = new CreateSubscriptionResponse(command, resulst);
            }

            try
            {
                var subscriptionGroups = command.MonitoredItems.GroupBy(item => item.PublishingInterval);
                var groupedItemsNotCreated = new Dictionary<string, List<string>>();

                foreach (var group in subscriptionGroups)
                {
                    var groupItems = group.ToList();
                    var batchHandler = new BatchHandler<SubscriptionMonitoredItem>(
                        opcUaConfiguration.SubscriptionBatchSize, 
                        this.SubscribeBatch(opcUaSession, groupedItemsNotCreated));

                    batchHandler.RunBatches(groupItems);
                    logger.LogTrace("Subscription with publishing interval {publishingInterval} ms: Subscribed to {nodes} nodes.", group.Key, groupItems.Count);
                }

                opcUaSession.ActivatePublishingOnAllSubscriptions();
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                errorMessages.Add((Bad, $"Could not create / update subscriptions. {error.Message}"));
            }

            //TODO: Move this code to the Repository
            if (!connectorConfiguration.DisableSubscriptionRestoreService && !errorMessages.Any())
            {
                var baseEndpointUrl = this.OpcSession.Session.GetBaseEndpointUrl();

                if (!errorMessages.Any())
                {
                    var monitoredItemsDict = command.MonitoredItems.ToDictionary(monitoredItem => monitoredItem.NodeId);
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
                logger.LogDebug("Created/Updated subscriptions on Endpoint: [{endpointUrl}]", command.EndpointUrl);

            return this.GetStatusMessage(errorMessages);
        }

        public Task<CommandResultBase> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResultBase> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #region [Privates]
        private async Task<List<(Opc.Ua.StatusCode, string)>> AddValidationErrorsAsync(CreateSubscriptionsCommand command)
        {
            var errorMessages = new List<(Opc.Ua.StatusCode, string)>();


            for (var itemIndex = 0; itemIndex < command.MonitoredItems.Length; itemIndex++)
            {
                var results = await monitoredItemValidator.ValidateAsync(command.MonitoredItems[itemIndex]);
                if (results.IsValid)
                    continue;

                var validationMessages = results.ToString(";");
                errorMessages.Add((Bad, $"Bad: {nameof(CreateSubscriptionsCommand.MonitoredItems)}[{itemIndex}]: {validationMessages}."));
            }
            return errorMessages;
        }

        private Action<SubscriptionMonitoredItem[]> SubscribeBatch(IOpcUaSession opcUaSession, Dictionary<string, List<string>> groupedItemsNotCreated)
        {
            return (monitoredItems) =>
            {
                Opc.Ua.Client.Subscription? opcUaSubscription = default;

                //TODO: Ask Hermo about this logic
                foreach (var monitoredItem in monitoredItems)
                {
                    opcUaSubscription = this.GetSubscription(monitoredItem);

                    opcUaSubscription = opcUaSession.CreateOrUpdateSubscription(monitoredItem);
                }

                try
                {
                    opcUaSubscription?.ApplyChanges();
                }
                catch (ServiceResultException sre)
                {
                    logger.LogError(sre, "Failed to call ApplyChanges() for batch with {monitoredItems} items: ", monitoredItems.Count());
                    throw;
                }

                
                this.CollectInvalidNodes(monitoredItems, opcUaSubscription, groupedItemsNotCreated);
            };
        }

        private void CollectInvalidNodes(
            SubscriptionMonitoredItem[] items, 
            Opc.Ua.Client.Subscription? opcUaSubscription,
            Dictionary<string, List<string>> itemsNotCreated)
        {
            if (opcUaSubscription is null)
                return;

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

                var success = itemsNotCreated.TryGetValue(key, out var list);
                if (success)
                    list!.AddRange(nodes);
                else
                {
                    list = nodes;
                    itemsNotCreated.Add(key, list);
                }
            }
        }

        private static string GetStatusMessage(List<string> errorMessages)
        {
            return errorMessages.Any()
                ? string.Join(" ", errorMessages)
                : "Good";
        }

        //TODO: ASK HERMO ABOUT THIS LOGIC
        private Opc.Ua.Client.Subscription? GetSubscription(SubscriptionMonitoredItem monitoredItem)
        {
            if (monitoredItem == default) 
                return null;

            var subscriptions = this.OpcSession.Session.Subscriptions
                .Where(x => x.MonitoredItems.Any(y => monitoredItem.NodeId.Equals(y.ResolvedNodeId.ToString())));

            return subscriptions.FirstOrDefault();
        }


        #endregion
    }
}
