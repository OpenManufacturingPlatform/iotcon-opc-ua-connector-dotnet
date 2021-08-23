using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Infrastructure.Kafka.States
{
    public class SubscriptionServiceStateManager : ISubscriptionServiceStateManager
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ISubscriptionServiceFactory _subscriptionServiceFactory;
        private ConcurrentDictionary<string, ISubscriptionService> _subscriptionServices;

        public SubscriptionServiceStateManager(ISubscriptionServiceFactory subscriptionServiceFactory)
        {
            this._subscriptionServices = new ConcurrentDictionary<string, ISubscriptionService>();
            this._semaphoreSlim = new SemaphoreSlim(1);
            this._subscriptionServiceFactory = subscriptionServiceFactory;
        }

        public async Task<ISubscriptionService> GetSubscriptionServiceInstanceAsync(string opcUaServerUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(opcUaServerUrl))
                return default;

            try
            {
                await this._semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (this._subscriptionServices.TryGetValue(opcUaServerUrl, out var service)) { return service; }

                service = this._subscriptionServiceFactory.Create();
                await this.AddServiceToDictionaryAsync(opcUaServerUrl, service, cancellationToken);

                return service;
            }
            finally
            {
                this._semaphoreSlim.Release();
            }
        }

        public async Task CleanupStaleServicesAsync()
        {
            //TODO: Needs implementation
            await Task.Yield();
        }

        private async Task AddServiceToDictionaryAsync(string endpointUrlKey, ISubscriptionService service, CancellationToken cancellationToken)
        {
            var serviceAddedToDictionary = false;
            while (!serviceAddedToDictionary)
            {
                cancellationToken.ThrowIfCancellationRequested();

                serviceAddedToDictionary = this._subscriptionServices.TryAdd(endpointUrlKey, service);

                if (!serviceAddedToDictionary)
                    await Task.Delay(1000, cancellationToken);
            }
        }

        public void Dispose()
        {
            this._subscriptionServices.Clear();
            this._subscriptionServices = null;
            this._semaphoreSlim?.Dispose();
        }
    }
}