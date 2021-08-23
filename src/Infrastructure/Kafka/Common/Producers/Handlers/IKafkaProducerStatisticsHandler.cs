using System;
using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Producers.Handlers
{

    public interface IKafkaProducerStatisticsHandler<TKey, TValue>
    {
        public void Handle(IProducer<TKey, TValue> producer, string statistics);
    }
}
