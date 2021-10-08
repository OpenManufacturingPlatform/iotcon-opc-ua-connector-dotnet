using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;
using OMP.Connector.Infrastructure.Kafka.Repositories;

namespace OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint
{
    public class ConfigurationConsumerHostedService : BaseConsumerHostedService
    {
        private const int OffsetRetrievalTimeoutSec = 5;
        private readonly IKafkaApplicationConfigurationRepository _applicationConfigurationRepository;
        private IConfigurationConsumer _configurationConsumer;

        public ConfigurationConsumerHostedService(
            IConsumerFactory consumerFactory,
            IKafkaApplicationConfigurationRepository applicationConfigurationRepository,
            ILogger<ConfigurationConsumerHostedService> logger)
            : base(consumerFactory, logger)
        {
            this._applicationConfigurationRepository = applicationConfigurationRepository;
        }

        protected override void Consume()
        {
            if (StoppingCancellationTokenSource.IsCancellationRequested)
                return;

            _configurationConsumer ??= ConsumerFactory.CreateConfigurationConsumer();
            _configurationConsumer.Consume(1000); //HACK to force the Subscribe() to be called
            
            var maxMessageOffset = GetMaximumOffset(_configurationConsumer.Consumer);
            InitConfiguration(maxMessageOffset);

            var consumeResult = _configurationConsumer.Consume(StoppingCancellationTokenSource.Token);
            var topicPartition = consumeResult.TopicPartition;
            var currentPosition = _configurationConsumer.Consumer.Position(topicPartition) - 1;

            var latestMessageOffset = GetMaximumOffset(_configurationConsumer.Consumer) - 1;
            if (!currentPosition.Equals(latestMessageOffset))
            {
                _configurationConsumer.Consumer.Commit(consumeResult);
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} committed intermediate config, sequence number: {currentPosition} ...");
                return;
            }

            Logger.LogInformation("**--CONSUME RESULT--**:\t{Key}:\t{Value}", consumeResult.Message.Key, consumeResult.Message.Value);

            _configurationConsumer.Consumer.Close();
            StoppingCancellationTokenSource.Cancel(true);

            Logger.LogInformation("**\tALL CONFIG READ OF TOPIC\tStopping Configuration consumer\t**");
        }

        private long GetMaximumOffset(IConsumer<string, AppConfigDto> consumer)
        {
            var currentMaximumOffset = Offset.Unset.Value;
            while (consumer.Assignment.Count == 0)
            {
                Thread.Sleep(1000); // TODO looks like a timeout setting regarding group coordination/partition assignment by the leader
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} consumer waiting to be assigned to partitions ...");
            }

            foreach (var topicPartition in consumer.Assignment)
            {
                var watermarkOffsets = consumer.QueryWatermarkOffsets(topicPartition, TimeSpan.FromSeconds(OffsetRetrievalTimeoutSec));
                currentMaximumOffset = Math.Max(currentMaximumOffset, watermarkOffsets.High);
            }
            return currentMaximumOffset;
        }

        private void InitConfiguration(long maxOffset)
        {
            if (maxOffset == 0)
            {
                _applicationConfigurationRepository.Initialize(new AppConfigDto
                {
                    Subscriptions = new List<SubscriptionDto>(),
                    EndpointDescriptions = new List<EndpointDescriptionDto>()
                });
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} published initial empty config to configuration topic");
            }
        }
    }
}