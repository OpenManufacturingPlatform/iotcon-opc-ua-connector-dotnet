// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

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

        public CustomKafkaConsumer(
            KafkaConfig kafkaConfig,
            ConsumerConfig configuration,
            ISerializerFactory serializerFactory = null,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
        {
            _kafkaConfiguration = kafkaConfig;
            var keyDeserializer = serializerFactory?.GetDeserializer<TKey>();
            var valueDeserializer = serializerFactory?.GetDeserializer<TValue>();

            var errorHandler = kafkaEventHandlerFactory?.GetConsumerErrorHandler<TKey, TValue>();
            var statisticsHandler = kafkaEventHandlerFactory?.GetConsumerStatisticsHandler<TKey, TValue>();
            var partitionsAssignedHandler = kafkaEventHandlerFactory?.GetConsumerPartitionsAssignedHandler<TKey, TValue>();
            var partitionsRevokeHandler = kafkaEventHandlerFactory?.GetConsumerPartitionsRevokeHandler<TKey, TValue>();
            var partitionsLostHandler = kafkaEventHandlerFactory?.GetConsumerPartitionsLostHandler<TKey, TValue>();
            var consumerLogHandler = kafkaEventHandlerFactory?.GetConsumerLogHandler<TKey, TValue>();

            var builder = new ConsumerBuilder<TKey, TValue>(configuration)
                    .SetErrorHandler((consumer, e) => errorHandler?.Handle(consumer, e))
                    .SetStatisticsHandler((consumer, json) => statisticsHandler?.Handle(consumer, json))
                    .SetPartitionsAssignedHandler((c, partitions) => partitionsAssignedHandler?.Handle(c, partitions))
                    .SetPartitionsRevokedHandler((c, partitions) => partitionsRevokeHandler?.Handle(c, partitions))
                    .SetPartitionsLostHandler((c, partitions) => partitionsLostHandler?.Handle(c, partitions))
                    .SetLogHandler((c, logMessage) => consumerLogHandler?.Handle(c, logMessage));

            if (keyDeserializer is not null)
                builder.SetKeyDeserializer(keyDeserializer);

            if (valueDeserializer is not null)
                builder.SetValueDeserializer(valueDeserializer);

            Consumer = builder.Build();
        }

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
           => Subscribe().Consumer.Consume(cancellationToken);

        public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
           => Subscribe().Consumer.Consume(timeout);

        public void Dispose()
        {
            try
            {
                Consumer?.Unsubscribe();
                Consumer?.Close();
                Consumer?.Dispose();
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;

                throw;
            }
        }

        private CustomKafkaConsumer<TKey, TValue> Subscribe()
        {

            var consumerSubscription = Consumer.Subscription;
            if (consumerSubscription is null || !consumerSubscription.Any())
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

    }
}