// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{
    public class KafkaProducerErrorHandler<TKey, TValue> : IKafkaProducerErrorHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaProducerErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        public virtual void Handle(IProducer<TKey, TValue> producer, Error error)
        {
            _logger.LogError($"Error: {error.Reason} | Consumer: {producer.Name}", error);
        }
    }
}
