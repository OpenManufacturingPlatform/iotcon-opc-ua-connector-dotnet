using System;
using System.Threading.Tasks;
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
            var appConfigDto = new AppConfigDto();
            try
            {
                if (StoppingCancellationTokenSource.IsCancellationRequested)
                    return;

                _configurationConsumer ??= ConsumerFactory.CreateConfigurationConsumer();

                var consumeResult = _configurationConsumer.Consume(5000);
                if (consumeResult is null)
                {
                    _applicationConfigurationRepository.Initialize(appConfigDto);
                    consumeResult = _configurationConsumer.Consume();
                }

                var latestMessageOffset = GetMaximumOffset(_configurationConsumer.Consumer) - 1;

                var topicPartition = consumeResult.TopicPartition;
                var currentPosition = _configurationConsumer.Consumer.Position(topicPartition) - 1;

                if (!currentPosition.Equals(latestMessageOffset))
                {
                    _configurationConsumer.Consumer.Commit(consumeResult);
                    Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} committed intermediate config, sequence number: {currentPosition} ...");
                    return;
                }

                Logger.LogInformation("**--CONSUME RESULT--**:\t{Key}:\t{Value}", consumeResult.Message.Key, consumeResult.Message.Value);

                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} notification for config sent, sequence number: {currentPosition}");
                _applicationConfigurationRepository.OnConfigChangeReceived(consumeResult);

                _configurationConsumer.Consumer.Close();
                StoppingCancellationTokenSource.Cancel(true);
                Logger.LogInformation("**\tALL CONFIG READ OF TOPIC\tStopping Configuration consumer\t**");
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Logger.LogWarning($"Consuming configuration was cancelled [{operationCanceledException.Message}]");
            }

            Logger.LogInformation("**\tConfiguration set in Repository\t**");
        }

        private long GetMaximumOffset(IConsumer<string, AppConfigDto> consumer)
        {
            var currentMaximumOffset = Offset.Unset.Value;
            while (consumer.Assignment.Count == 0)
            {
                Task.Delay(1000).GetAwaiter().GetResult(); // TODO looks like a timeout setting regarding group coordination/partition assignment by the leader
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} consumer waiting to be assigned to partitions ...");
            }

            foreach (var topicPartition in consumer.Assignment)
            {
                var watermarkOffsets = consumer.QueryWatermarkOffsets(topicPartition, TimeSpan.FromSeconds(OffsetRetrievalTimeoutSec));
                currentMaximumOffset = Math.Max(currentMaximumOffset, watermarkOffsets.High);
            }
            return currentMaximumOffset;
        }
    }
}