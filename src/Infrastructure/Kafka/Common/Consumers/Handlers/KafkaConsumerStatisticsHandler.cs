// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerStatisticsHandler<TKey, TValue> : IKafkaConsumerStatisticsHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerStatisticsHandler(ILogger logger)
        {
            _logger = logger;
        }
        public virtual void Handle(IConsumer<TKey, TValue> consumer, string statistics)
        {
            var jsonStatistics = JsonConvert.DeserializeObject(statistics);
            _logger.LogTrace($"[StatisticsHandler]\tConsumer: {consumer.Name}\t|\tSubscription: {string.Join(",", consumer.Subscription)}", jsonStatistics);
        }
    }
}