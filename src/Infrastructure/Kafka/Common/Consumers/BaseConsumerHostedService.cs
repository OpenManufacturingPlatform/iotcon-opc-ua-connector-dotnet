using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Extensions;

namespace OMP.Device.Connector.Kafka.Common.Consumers
{
    public abstract class BaseConsumerHostedService : IHostedService
    {
        protected readonly IConsumerFactory ConsumerFactory;
        protected readonly ILogger Logger;
        private Task _backgroundTask;
        protected CancellationTokenSource StoppingCancellationTokenSource;

        public BaseConsumerHostedService(IConsumerFactory consumerFactory, ILogger logger)
        {
            ConsumerFactory = consumerFactory;
            Logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            StoppingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1]
            {
                cancellationToken
            });

            _backgroundTask = Task.Run(() => { StartConsumerLoop(); }, StoppingCancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                StoppingCancellationTokenSource.Cancel(true);
                while (_backgroundTask.Status == TaskStatus.Running)
                    cancellationToken.ThrowIfCancellationRequested();

                return Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug("Could not stop consumer thread in time - Aborting thread");
            }

            return Task.CompletedTask;
        }

        protected abstract void Consume();

        private void StartConsumerLoop()
        {
            while (!StoppingCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Logger.LogDebug("Attempting to consume a message");
                    Consume();
                }
                catch (OperationCanceledException)
                {
                    Logger.LogDebug("Consuming of a message timed out");
                    break;
                }
                catch (ConsumeException cx)
                {
                    var message = cx.GetMessage();
                    Logger.LogDebug($"Unable to Consumer message:\t{message}", cx);
                }
                catch (Exception e)
                {
                    var message = e.GetMessage();
                    Logger.LogError(e, "Unexpected error: {Message}", message);
                }
            }
        }
    }
}
