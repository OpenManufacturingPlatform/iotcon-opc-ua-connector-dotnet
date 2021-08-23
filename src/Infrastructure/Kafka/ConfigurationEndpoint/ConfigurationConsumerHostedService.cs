using System;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Consumers;
using OMP.Device.Connector.Kafka.Repositories;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
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

            if (_configurationConsumer is null)
                _configurationConsumer = ConsumerFactory.CreateConfigurationConsumer();

            var offsetsCheckedForConsumptionOnce = false;
            var latestMessageOffset = Offset.Unset.Value;

            var consumeResult = _configurationConsumer.Consume(StoppingCancellationTokenSource.Token);

            if (!offsetsCheckedForConsumptionOnce)
            {
                latestMessageOffset = GetMaximumOffset(_configurationConsumer.Consumer) - 1;
                offsetsCheckedForConsumptionOnce = true;
            }

            var topicPartition = consumeResult.TopicPartition;
            var currentPosition = _configurationConsumer.Consumer.Position(topicPartition) - 1;

            if (!currentPosition.Equals(latestMessageOffset))
            {
                _configurationConsumer.Consumer.Commit(consumeResult);
                Logger.LogTrace($"{nameof(ConfigurationConsumerHostedService)} committed intermediate config, sequence number: {currentPosition} ...");
                return;
            }

            Logger.LogInformation("**--CONSUME RESULT--**:\t{Key}:\t{Value}", consumeResult.Message.Key, consumeResult.Message.Value);

            _applicationConfigurationRepository.Initialize(consumeResult.Message?.Value);
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
                Logger.LogTrace($"{nameof(IConfigurationConsumer)} consumer waiting to be assigned to partitions ...");
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
