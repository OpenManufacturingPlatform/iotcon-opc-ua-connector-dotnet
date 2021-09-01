using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Device.Connector.Kafka.Common.Producers.Handlers
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
