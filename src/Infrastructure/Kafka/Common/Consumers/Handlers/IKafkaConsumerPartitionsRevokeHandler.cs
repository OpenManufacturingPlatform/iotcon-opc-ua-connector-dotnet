// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerPartitionsRevokeHandler<TKey, TValue>
    {
        public void Handle(IConsumer<TKey, TValue> consumer, List<TopicPartitionOffset> partitions);
    }
}
