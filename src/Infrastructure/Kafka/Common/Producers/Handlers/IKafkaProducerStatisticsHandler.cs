// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{

    public interface IKafkaProducerStatisticsHandler<TKey, TValue>
    {
        public void Handle(IProducer<TKey, TValue> producer, string statistics);
    }
}
