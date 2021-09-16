using Microsoft.Extensions.Logging;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers;

namespace OMP.Connector.Infrastructure.Kafka.Common.Events
{
    public class DefaultKafkaEventHandlerFactory : IKafkaEventHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public DefaultKafkaEventHandlerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        #region [Consumers]
        public IKafkaConsumerErrorHandler<TKey, TValue> GetConsumerErrorHandler<TKey, TValue>()
           => new KafkaConsumerErrorHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaConsumerPartitionsAssignedHandler<TKey, TValue> GetConsumerPartitionsAssignedHandler<TKey, TValue>()
            => new KafkaConsumerPartitionsAssignedHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaConsumerPartitionsLostHandler<TKey, TValue> GetConsumerPartitionsLostHandler<TKey, TValue>()
            => new KafkaConsumerPartitionsLostHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaConsumerPartitionsRevokeHandler<TKey, TValue> GetConsumerPartitionsRevokeHandler<TKey, TValue>()
            => new KafkaConsumerPartitionsRevokeHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaConsumerStatisticsHandler<TKey, TValue> GetConsumerStatisticsHandler<TKey, TValue>()
            => new KafkaConsumerStatisticsHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaConsumerLogHandler<TKey, TValue> GetConsumerLogHandler<TKey, TValue>()
            => new KafkaConsumerLogHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        #endregion

        #region [Producer]
        public IKafkaProducerErrorHandler<TKey, TValue> GetProducerErrorHandler<TKey, TValue>()
            => new KafkaProducerErrorHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaProducerStatisticsHandler<TKey, TValue> GetProducerStatisticsHandler<TKey, TValue>()
            => new KafkaProducerStatisticsHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        public IKafkaProducerLogHandler<TKey, TValue> GetProducerLogHandler<TKey, TValue>()
            => new KafkaProducerLogHandler<TKey, TValue>(_loggerFactory.CreateLogger<TValue>());

        #endregion
    }
}
