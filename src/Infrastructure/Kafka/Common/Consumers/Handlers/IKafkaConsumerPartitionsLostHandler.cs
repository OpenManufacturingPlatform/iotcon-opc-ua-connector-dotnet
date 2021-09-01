using System.Collections.Generic;
using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerPartitionsLostHandler<TKey, TValue>
    {
        public void Handle(IConsumer<TKey, TValue> consumer, List<TopicPartitionOffset> partitions);
    }
}
