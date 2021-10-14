using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.Extensions;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{
    public partial class KafkaRepository : IKafkaApplicationConfigurationRepository
    {
        private readonly IDictionary<(string EndpointUrl, string PublishingInterval), SubscriptionDto> _managedSubscriptions;
        private readonly IDictionary<string, EndpointDescriptionDto> _endpointDescriptions;
        private readonly ILogger<KafkaRepository> _logger;
        private readonly IConfigurationPersister _configurationPersister;
        private bool _repositoryInitialized;

        public KafkaRepository(ILogger<KafkaRepository> logger, IConfigurationPersister configurationPersister)
        {
            _logger = logger;
            _configurationPersister = configurationPersister;
            _managedSubscriptions = new Dictionary<(string EndpointUrl, string PublishingInterval), SubscriptionDto>();
            _endpointDescriptions = new Dictionary<string, EndpointDescriptionDto>();
            _repositoryInitialized = false;
        }

        public void Initialize(AppConfigDto applicationConfig)
        {
            if (!applicationConfig.Subscriptions.Any() && !applicationConfig.EndpointDescriptions.Any())
                PersistCachedConfig();

            if (applicationConfig.Subscriptions is not null)
                foreach (var subscriptionDto in applicationConfig.Subscriptions)
                {
                    UpdateSubscriptionCache(subscriptionDto, false);
                }

            if (applicationConfig.EndpointDescriptions is not null)
                foreach (var descriptionDto in applicationConfig.EndpointDescriptions)
                {
                    if (!_endpointDescriptions.TryGetValue(descriptionDto.EndpointUrl, out _))
                        _endpointDescriptions.Add(descriptionDto.EndpointUrl, descriptionDto);
                }

            _repositoryInitialized = true;
        }

        private bool PersistCachedConfig()
        {
            var newConfig = new AppConfigDto
            {
                Subscriptions = _managedSubscriptions.Values.ToList(),
                EndpointDescriptions = _endpointDescriptions.Values.ToList()
            };

            var result = _configurationPersister.SaveConfigurationAsync(newConfig, default).GetAwaiter().GetResult();

            return result.Match(
                success =>
                {
                    _logger.LogTrace($"{nameof(KafkaRepository)} successfully updated the configuration on the Topic");
                    return true;
                },
                partialSuccess =>
                {
                    _logger.LogTrace($"{nameof(KafkaRepository)} attempted to update config, wich partially succeeded: Result = {partialSuccess.Message}");
                    return false;
                },
                messageToLarge =>
                {
                    _logger.LogTrace($"{nameof(KafkaRepository)} attempted to update config, wich failed [message too large]: Result = {messageToLarge.Error}");
                    return false;
                },
                errorResult =>
                {
                    _logger.LogTrace($"{nameof(KafkaRepository)} attempted to update config, wich failed: Result = {errorResult.Error}");
                    return false;
                });
        }

        private IEnumerable<SubscriptionDto> GroupByPublishingInterval(SubscriptionDto subscription)
        {
            if (subscription.PublishingInterval != null)
                return new List<SubscriptionDto> { subscription };

            return subscription.MonitoredItems
                .GroupBy(x => x.Value.PublishingInterval)
                .Select(g => new SubscriptionDto
                {
                    EndpointUrl = subscription.EndpointUrl,
                    PublishingInterval = g.Key,
                    MonitoredItems = g.ToDictionary(v => v.Key, v => v.Value)
                });
        }

        private bool UpdateSubscriptionCache(SubscriptionDto candidateSubscription, bool overrideExistingItem)
        {
            if (overrideExistingItem)
                RemoveMonitoredItemsFromCache(candidateSubscription.EndpointUrl, candidateSubscription.MonitoredItems.Values, candidateSubscription.PublishingInterval);

            var key = (candidateSubscription.EndpointUrl, candidateSubscription.PublishingInterval);
            if (!_managedSubscriptions.TryGetValue(key, out var cachedSubscription))
            {
                _managedSubscriptions.Add(key, candidateSubscription);
                return true;
            }

            var candidateNodeIds = candidateSubscription.MonitoredItems.Keys;
            foreach (var candidateNodeId in candidateNodeIds)
            {
                candidateSubscription.MonitoredItems.TryGetValue(candidateNodeId, out var candidateMonitoredItem);

                if (!cachedSubscription.MonitoredItems.TryGetValue(candidateNodeId, out var cachedMonitoredItem))
                {
                    cachedSubscription.MonitoredItems.Add(candidateNodeId, candidateMonitoredItem);
                    return true;
                }

                if (!overrideExistingItem || cachedMonitoredItem.EqualsByValue(candidateMonitoredItem))
                    continue;

                cachedSubscription.MonitoredItems[candidateNodeId] = candidateMonitoredItem;
                return true;
            }
            return false;
        }

        private bool RemoveMonitoredItemsFromCache(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items, string publishingInterval = null)
        {
            var itemsToDelete = items.ToList();

            var cacheChanged = false;

            foreach ((var key, SubscriptionDto subscriptionDto) in this._managedSubscriptions)
            {
                if (!key.EndpointUrl.Equals(endpointUrl, StringComparison.OrdinalIgnoreCase))
                    continue;

                cacheChanged = itemsToDelete
                    .Select(x => x.NodeId)
                    .Aggregate(
                        cacheChanged,
                        (current, nodeId) =>
                        {
                            if (subscriptionDto.MonitoredItems.Any(m => HasThePublishingIntervalChanged(publishingInterval, nodeId, m)))//this is the fix for config growth
                                return subscriptionDto.MonitoredItems.Remove(nodeId) || current;

                            return current;
                        });

                if (subscriptionDto.MonitoredItems.Count == 0)
                    _managedSubscriptions.Remove(key);
            }

            return cacheChanged;
        }

        private static bool HasThePublishingIntervalChanged(string publishingInterval, string nodeId, KeyValuePair<string, SubscriptionMonitoredItem> m)
           => NodeIdIsTheSame(nodeId, m) && PublishingIntervalsIsDifferent(publishingInterval, m);

        private static bool PublishingIntervalsIsDifferent(string publishingInterval, KeyValuePair<string, SubscriptionMonitoredItem> m)
            => publishingInterval is null
               ||
               m.Value.PublishingInterval != publishingInterval;

        private static bool NodeIdIsTheSame(string nodeId, KeyValuePair<string, SubscriptionMonitoredItem> m)
            => m.Key == nodeId;       
    }
}
