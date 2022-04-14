// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{
    public class KafkaProducerLogHandler<TKey, TValue> : IKafkaProducerLogHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaProducerLogHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(IProducer<TKey, TValue> producer, LogMessage logMessage)
        {
            _logger.LogTrace($"Producer: {logMessage.Name}\t\t| Message: {logMessage.Message}", logMessage);
        }
    }
}
