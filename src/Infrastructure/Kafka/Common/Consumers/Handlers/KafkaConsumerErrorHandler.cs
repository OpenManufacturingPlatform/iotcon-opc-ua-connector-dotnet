using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Device.Connector.Kafka.Common.Consumers.Handlers
{
    public class KafkaConsumerErrorHandler<TKey, TValue> : IKafkaConsumerErrorHandler<TKey, TValue>
    {
        private readonly ILogger _logger;

        public KafkaConsumerErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        public virtual void Handle(IConsumer<TKey, TValue> consumer, Error error)
        {
            _logger.LogError($"Error: {error.Reason} | Consumer: {consumer.Name} | Subscription: {string.Join(",", consumer.Subscription)}", error);
        }
    }
}
