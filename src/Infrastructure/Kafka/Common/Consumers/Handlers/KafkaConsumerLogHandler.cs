﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerLogHandler<TKey, TValue> : IKafkaConsumerLogHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerLogHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(IConsumer<TKey, TValue> consumer, LogMessage logMessage)
        {
            _logger.LogTrace($"Consumer: {logMessage.Name}\t\t| Message: {logMessage.Message}", logMessage);
        }
    }
}