// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;

namespace OMP.Connector.Infrastructure.Kafka.CommandEndpoint
{
    public class CommandConsumerHostedService : BaseConsumerHostedService
    {
        private readonly IKafkaRequestHandler _kafkaRequestHandler;
        private ICommandConsumer _commandConsumer;
        public CommandConsumerHostedService(
            IConsumerFactory consumerFactory,
            IKafkaRequestHandler kafkaRequestHandler,
            ILogger<CommandConsumerHostedService> logger)
            : base(consumerFactory, logger)
        {
            this._kafkaRequestHandler = kafkaRequestHandler;
        }

        protected override void Consume()
        {
            try
            {
                _commandConsumer ??= ConsumerFactory.CreateCommandConsumer();

                var consumeResult = _commandConsumer.Consume(StoppingCancellationTokenSource.Token);
                Logger.LogInformation("{Key}: {Value}", consumeResult.Message.Key, consumeResult.Message.Value);
                _kafkaRequestHandler.OnMessageReceived(consumeResult);
                _commandConsumer.Consumer.Commit(consumeResult);
            }
            catch (ConsumeException cx)
            {
                var message = cx.GetMessage();
                Logger.LogDebug($"Unable to consume message:\t{message}", cx);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void StopConsumer()
        {
            _commandConsumer?.Consumer?.Close();
        }
    }
}