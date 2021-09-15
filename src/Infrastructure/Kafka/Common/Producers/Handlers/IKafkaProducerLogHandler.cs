using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{
    public interface IKafkaProducerLogHandler<TKey, TValue>
    {
        void Handle(IProducer<TKey, TValue> producer, LogMessage logMessage);
    }
}
