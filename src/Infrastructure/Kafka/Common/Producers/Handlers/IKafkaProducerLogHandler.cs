using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Producers.Handlers
{
    public interface IKafkaProducerLogHandler<TKey, TValue>
    {
        void Handle(IProducer<TKey, TValue> producer, LogMessage logMessage);
    }
}
