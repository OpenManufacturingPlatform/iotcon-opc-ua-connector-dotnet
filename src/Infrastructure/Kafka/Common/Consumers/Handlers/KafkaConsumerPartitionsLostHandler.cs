// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerPartitionsLostHandler<TKey, TValue> : IKafkaConsumerPartitionsLostHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerPartitionsLostHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(IConsumer<TKey, TValue> consumer, List<TopicPartitionOffset> partitions)
        {
            // The lost partitions handler is called when the consumer detects that it has lost ownership
            // of its assignment (fallen out of the group).
            _logger.LogTrace($"Partitions were lost: [{string.Join(", ", partitions)}] || Consumer: {consumer.Name} | Subscription: {string.Join(",", consumer.Subscription)}");
        }
    }
}