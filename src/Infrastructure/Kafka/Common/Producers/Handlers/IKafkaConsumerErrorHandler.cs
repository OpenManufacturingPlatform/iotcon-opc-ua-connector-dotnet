using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Producers.Handlers
{
    public interface IKafkaProducerErrorHandler<TKey, TValue>
    {
        public void Handle(IProducer<TKey, TValue> producer, Error error);
    } 
}
