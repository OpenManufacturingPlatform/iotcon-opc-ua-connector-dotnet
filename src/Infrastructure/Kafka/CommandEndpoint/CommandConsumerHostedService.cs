using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Extensions;
using OMP.Device.Connector.Kafka.Common.Consumers;

namespace OMP.Device.Connector.Kafka.CommandEndpoint
{
    public class CommandConsumerHostedService : BaseConsumerHostedService
    {
        private readonly IKafkaRequestHandler _kafkaRequestHandler;
        private ICommandConsumer commandConsumer;
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
                if (commandConsumer is null)
                    commandConsumer = ConsumerFactory.CreateCommandConsumer();

                var consumeResult = commandConsumer.Consume(StoppingCancellationTokenSource.Token);
                Logger.LogInformation("{Key}: {Value}", consumeResult.Message.Key, consumeResult.Message.Value);
                _kafkaRequestHandler.OnMessageReceived(consumeResult);
                commandConsumer.Consumer.Commit(consumeResult);
            }
            catch (ConsumeException cx)
            {
                var message = cx.GetMessage();
                Logger.LogDebug($"Unable to Consumer message:\t{message}", cx);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}