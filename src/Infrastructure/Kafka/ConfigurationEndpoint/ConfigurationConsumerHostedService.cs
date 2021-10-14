﻿using System;
using System.Threading;
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

                var consumeResult = GetConsumeResult();
                if (consumeResult is null)
                {
                    _applicationConfigurationRepository.Initialize(appConfigDto);
                    return;
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
                _applicationConfigurationRepository.Initialize(consumeResult.Message?.Value);

                _configurationConsumer.Consumer.Close();
                SignalOuterLoopToStopConsumption();
                Logger.LogInformation("**\tALL CONFIG READ OF TOPIC\tStopping Configuration consumer\t**");
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Logger.LogWarning($"Consuming configuration was cancelled [{operationCanceledException.Message}]");
            }

            Logger.LogInformation("**\tConfiguration set in Repository\t**");
        }

        private void SignalOuterLoopToStopConsumption()
            => StoppingCancellationTokenSource.Cancel(true);

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

        private ConsumeResult<string, AppConfigDto> GetConsumeResult(int timeout = OffsetRetrievalTimeoutSec)
        {
            var s_cts = new CancellationTokenSource();
            s_cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            return GetConsumeResult(s_cts.Token);
        }

        private ConsumeResult<string, AppConfigDto> GetConsumeResult(CancellationToken cancellationToken)
        {
            try
            {
                return _configurationConsumer.Consume(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)}.{nameof(GetConsumeResult)} timed out");
                return default;
            }
        }
    }
}