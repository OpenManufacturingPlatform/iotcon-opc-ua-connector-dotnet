using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerPartitionsRevokeHandler<TKey, TValue> : IKafkaConsumerPartitionsRevokeHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerPartitionsRevokeHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(IConsumer<TKey, TValue> consumer, List<TopicPartitionOffset> partitions)
        {
            // Since a cooperative assignor (CooperativeSticky) has been configured, the revoked
            // assignment is incremental (may remove only some partitions of the current assignment).
            var remaining = consumer.Assignment.Where(atp => partitions.Where(rtp => rtp.TopicPartition == atp).Count() == 0);
            _logger.LogTrace(
                "Partitions incrementally revoked: [" +
                string.Join(',', partitions.Select(p => p.Partition.Value)) +
                "], remaining: [" +
                string.Join(',', remaining.Select(p => p.Partition.Value)) +
                "]");
        }
    }
}