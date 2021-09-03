using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Device.Connector.Kafka.Common.Producers.Handlers
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
