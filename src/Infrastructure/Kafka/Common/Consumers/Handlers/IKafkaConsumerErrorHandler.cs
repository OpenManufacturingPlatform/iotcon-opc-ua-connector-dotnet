using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerErrorHandler<TKey, TValue>
    {
        public void Handle(IConsumer<TKey, TValue> consumer, Error error);
    }
}
