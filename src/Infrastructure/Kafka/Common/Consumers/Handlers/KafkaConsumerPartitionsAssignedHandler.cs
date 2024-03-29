﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerPartitionsAssignedHandler<TKey, TValue> : IKafkaConsumerPartitionsAssignedHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerPartitionsAssignedHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(IConsumer<TKey, TValue> consumer, List<TopicPartition> partitions)
        {
            // Since a cooperative assignor (CooperativeSticky) has been configured, the
            // partition assignment is incremental (adds partitions to any existing assignment).
            _logger.LogTrace(
                "Partitions incrementally assigned: [" +
                string.Join(',', partitions.Select(p => p.Partition.Value)) +
                "], all: [" +
                string.Join(',', consumer.Assignment.Concat(partitions).Select(p => p.Partition.Value)) +
                "]");
        }
    }
}