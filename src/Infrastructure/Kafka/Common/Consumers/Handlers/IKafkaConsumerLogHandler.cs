using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerLogHandler<TKey, TValue>
    {
        void Handle(IConsumer<TKey, TValue> producer, LogMessage logMessage);
    }
}
