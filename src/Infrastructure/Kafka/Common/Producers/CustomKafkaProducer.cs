using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application;
using OMP.Connector.Domain.Exceptions;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses;
using OMP.Connector.Infrastructure.Kafka.Serialization;
using OneOf;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers
{
    public class CustomKafkaProducer<TKey, TValue> : ICustomKafkaProducer<TKey, TValue>
    {
        protected readonly KafkaConfig Config;
        protected readonly ILogger Logger;
        private ProducerBuilder<TKey, TValue> _builder;

        public IProducer<TKey, TValue> Producer { get; }

        public CustomKafkaProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger logger,
            ISerializerFactory serializerFactory = null,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
        {
            if (kafkaConfig == null)
                throw new ArgumentNullException(nameof(kafkaConfig));

            var keySeserializer = serializerFactory.GetSeserializer<TKey>();
            var valueSeserializer = serializerFactory.GetSeserializer<TValue>();

            var errorHandler = kafkaEventHandlerFactory?.GetProducerErrorHandler<TKey, TValue>();
            var statisticsHandler = kafkaEventHandlerFactory?.GetProducerStatisticsHandler<TKey, TValue>();
            var producerLogHandler = kafkaEventHandlerFactory?.GetProducerLogHandler<TKey, TValue>();

            Config = kafkaConfig;
            this.Logger = logger;
            _builder = new ProducerBuilder<TKey, TValue>(configuration)
                .SetErrorHandler((consumer, e) => errorHandler?.Handle(consumer, e))
                    .SetStatisticsHandler((consumer, json) => statisticsHandler?.Handle(consumer, json))
                    .SetLogHandler((p, logMessage) => producerLogHandler?.Handle(p, logMessage));

            if (keySeserializer is not null)
                _builder.SetKeySerializer(keySeserializer);

            if (valueSeserializer is not null)
                _builder.SetValueSerializer(valueSeserializer);

            Producer = _builder.Build();
        }

        public Message<TKey, TValue> CreateMessage(TKey key, TValue value, Headers headers = null)
        {
            var message = CreateMessage(value, headers);
            message.Key = key;
            return message;
        }
        public Message<TKey, TValue> CreateMessage(TValue value, Headers headers = null)
        {
            var dateTime = DateTimeOffset.UtcNow;
            var timestamp = new Timestamp(dateTime);

            return new Message<TKey, TValue>
            {
                Value = value,
                Timestamp = timestamp,
                Headers = headers
            };
        }

        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> ProduceAsync(TValue message, CancellationToken cancellationToken = default)
            => ProduceAsync(CreateMessage(message), cancellationToken);

        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> ProduceAsync(TKey key, TValue message, CancellationToken cancellationToken = default)
           => ProduceAsync(CreateMessage(key, message), cancellationToken);

        private async Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> ProduceAsync(Message<TKey, TValue> message, CancellationToken cancellationToken = default)
        {
            try
            {
                DeliveryResult<TKey, TValue> result = null;
                if (Config.MaxRetryAttempts > 0)
                {
                    await RetryHelper.RetryOnExceptionAsync<Exception>(
                        Config.MaxRetryAttempts,
                        Config.PauseBetweenFailures,
                        async () =>
                        {
                            result = await Producer.ProduceAsync(Config.Topic, message, cancellationToken);
                        },
                        LogRetryException,
                        MessageSizeIsTooLarge);
                }
                else
                {
                    result = await Producer.ProduceAsync(Config.Topic, message, cancellationToken);
                }

                return result.Status switch
                {
                    PersistenceStatus.Persisted => new PublishSucceeded(),
                    PersistenceStatus.PossiblyPersisted => new PublishPartialSucceeded(result.Status.ToString()),
                    _ => new PublishFailed(result.Status.ToString())
                };
            }
            catch (RetryFailedException rfe)
            {
                if (MessageSizeIsTooLarge(rfe))
                    return new PublishedFailedMessageSizeTooLarge(rfe);

                return new PublishFailed($"Failed to publish message after {rfe.RetryAttempt} attempt(s)", rfe);
            }
            catch (RetryAbortedException rae)
            {
                if (MessageSizeIsTooLarge(rae))
                    return new PublishedFailedMessageSizeTooLarge(rae);

                return new PublishFailed($"Aborted publishing of message after {rae.RetryAttempt} attempt(s)", rae);
            }
            catch (Exception ex)
            {
                if (MessageSizeIsTooLarge(ex))
                    return new PublishedFailedMessageSizeTooLarge(ex);

                return new PublishFailed(ex);
            }
        }

        public void Dispose()
        {
            // Block until all outstanding produce requests have completed (with or without error).
            try
            {
                Producer?.Flush();
                Producer?.Dispose();
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;

                throw;
            }
        }

        protected void LogRetryException(Exception e, int attemptCounter)
            => Logger.LogTrace($"Failed to publish message on attempt: {attemptCounter}", e);

        protected bool MessageSizeIsTooLarge(Exception ex)
            => ex is KafkaException kex && kex.Error.Code == ErrorCode.MsgSizeTooLarge;       
    }
}
