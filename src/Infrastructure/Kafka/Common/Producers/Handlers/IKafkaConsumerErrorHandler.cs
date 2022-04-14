// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{
    public interface IKafkaProducerErrorHandler<TKey, TValue>
    {
        public void Handle(IProducer<TKey, TValue> producer, Error error);
    }
}
