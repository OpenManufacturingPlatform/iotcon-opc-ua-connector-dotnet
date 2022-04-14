// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public interface IKafkaConsumerLogHandler<TKey, TValue>
    {
        void Handle(IConsumer<TKey, TValue> producer, LogMessage logMessage);
    }
}
