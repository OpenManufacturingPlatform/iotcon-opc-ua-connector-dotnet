using OMP.Connector.Infrastructure.Kafka.Common.Consumers.Handlers;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Handlers;

namespace OMP.Connector.Infrastructure.Kafka.Common.Events
{
    public interface IKafkaEventHandlerFactory
    {
        #region [Consumer]
        public IKafkaConsumerErrorHandler<TKey, TValue> GetConsumerErrorHandler<TKey, TValue>();
        public IKafkaConsumerStatisticsHandler<TKey, TValue> GetConsumerStatisticsHandler<TKey, TValue>();
        public IKafkaConsumerPartitionsAssignedHandler<TKey, TValue> GetConsumerPartitionsAssignedHandler<TKey, TValue>();
        public IKafkaConsumerPartitionsRevokeHandler<TKey, TValue> GetConsumerPartitionsRevokeHandler<TKey, TValue>();
        public IKafkaConsumerPartitionsLostHandler<TKey, TValue> GetConsumerPartitionsLostHandler<TKey, TValue>();
        public IKafkaConsumerLogHandler<TKey, TValue> GetConsumerLogHandler<TKey, TValue>();
        #endregion

        #region [Producer]
        public IKafkaProducerErrorHandler<TKey, TValue> GetProducerErrorHandler<TKey, TValue>();
        public IKafkaProducerStatisticsHandler<TKey, TValue> GetProducerStatisticsHandler<TKey, TValue>();
        public IKafkaProducerLogHandler<TKey, TValue> GetProducerLogHandler<TKey, TValue>();
        #endregion
    }
}
