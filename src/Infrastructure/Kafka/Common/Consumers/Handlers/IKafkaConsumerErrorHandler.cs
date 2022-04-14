// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerErrorHandler<TKey, TValue>
    {
        public void Handle(IConsumer<TKey, TValue> consumer, Error error);
    }
}
