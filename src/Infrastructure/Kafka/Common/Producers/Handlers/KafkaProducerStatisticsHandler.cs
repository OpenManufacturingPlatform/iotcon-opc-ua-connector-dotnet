// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers
{
    public class KafkaProducerStatisticsHandler<TKey, TValue> : IKafkaProducerStatisticsHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaProducerStatisticsHandler(ILogger logger)
        {
            _logger = logger;
        }
        public virtual void Handle(IProducer<TKey, TValue> producer, string statistics)
        {
            var jsonStatistics = JsonConvert.DeserializeObject(statistics);
            _logger.LogTrace($"[StatisticsHandler]\tProducer: {producer.Name}", jsonStatistics);
        }
    }
}
