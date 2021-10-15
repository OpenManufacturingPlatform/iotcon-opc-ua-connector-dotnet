using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers
{
    public class CustomKafkaConsumer<TKey, TValue> : IDisposable, ICustomKafkaConsumer<TKey, TValue>
    {
        public IConsumer<TKey, TValue> Consumer { get; }

        private readonly KafkaConfig _kafkaConfiguration;
        private ConsumerConfig _configuration;

        public CustomKafkaConsumer(
            KafkaConfig kafkaConfig,
            ConsumerConfig configuration,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
        {
            _kafkaConfiguration = kafkaConfig;
            var keyDeserializer = serializerFactory.GetDeserializer<TKey>();
            var valueDeserializer = serializerFactory.GetDeserializer<TValue>();

            var errorHandler = kafkaEventHandlerFactory.GetConsumerErrorHandler<TKey, TValue>();
            var statisticsHandler = kafkaEventHandlerFactory.GetConsumerStatisticsHandler<TKey, TValue>();
            var partitionsAssignedHandler = kafkaEventHandlerFactory.GetConsumerPartitionsAssignedHandler<TKey, TValue>();
            var partitionsRevokeHandler = kafkaEventHandlerFactory.GetConsumerPartitionsRevokeHandler<TKey, TValue>();
            var partitionsLostHandler = kafkaEventHandlerFactory.GetConsumerPartitionsLostHandler<TKey, TValue>();
            var consumerLogHandler = kafkaEventHandlerFactory.GetConsumerLogHandler<TKey, TValue>();

            _configuration = configuration;
            var builder = new ConsumerBuilder<TKey, TValue>(configuration)
                    .SetErrorHandler((consumer, e) => errorHandler?.Handle(consumer, e))
                    .SetStatisticsHandler((consumer, json) => statisticsHandler?.Handle(consumer, json))
                    .SetPartitionsAssignedHandler((c, partitions) => partitionsAssignedHandler?.Handle(c, partitions))
                    .SetPartitionsRevokedHandler((c, partitions) => partitionsRevokeHandler?.Handle(c, partitions))
                    .SetPartitionsLostHandler((c, partitions) => partitionsLostHandler?.Handle(c, partitions))
                    .SetKeyDeserializer(keyDeserializer)
                    .SetValueDeserializer(valueDeserializer)
                    .SetLogHandler((c, logMessage) => consumerLogHandler?.Handle(c, logMessage));
            Consumer = builder.Build();
        }

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
           => Subscribe().Consumer.Consume(cancellationToken);

        private CustomKafkaConsumer<TKey, TValue> Subscribe()
        {

            var consumerSubscription = Consumer.Subscription;
            if (consumerSubscription == null || !consumerSubscription.Any())
            {
                Consumer.Subscribe(_kafkaConfiguration.Topics);
                return this;
            }

            consumerSubscription.Sort(StringComparer.Ordinal);

            if (_kafkaConfiguration.Topics.SequenceEqual(consumerSubscription))
                return this;

            Consumer.Unsubscribe();
            Consumer.Subscribe(_kafkaConfiguration.Topics);

            return this;
        }

        public void Dispose()
        {
            try
            {
                Consumer?.Close();
                Consumer?.Dispose();
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;

                throw;
            }
        }
    }
}
