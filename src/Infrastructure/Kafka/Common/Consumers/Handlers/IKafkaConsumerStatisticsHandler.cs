using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerStatisticsHandler<TKey, TValue>
    {
        public void Handle(IConsumer<TKey, TValue> consumer, string statistics);
    }
}
