using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Infrastructure.OpcUa.States
{
    public class AlarmSubscriptionServiceStateManager : IAlarmSubscriptionServiceStateManager
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IAlarmSubscriptionServiceFactory _subscriptionServiceFactory;
        private ConcurrentDictionary<string, IAlarmSubscriptionService> _subscriptionServices;

        public AlarmSubscriptionServiceStateManager(IAlarmSubscriptionServiceFactory subscriptionServiceFactory)
        {
            _subscriptionServices = new ConcurrentDictionary<string, IAlarmSubscriptionService>();
            _semaphoreSlim = new SemaphoreSlim(1);
            _subscriptionServiceFactory = subscriptionServiceFactory;
        }

        public async Task<IAlarmSubscriptionService> GetAlarmSubscriptionServiceInstanceAsync(string opcUaServerUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(opcUaServerUrl))
                return default;

            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (_subscriptionServices.TryGetValue(opcUaServerUrl, out var service)) { return service; }

                service = _subscriptionServiceFactory.Create();
                await AddServiceToDictionaryAsync(opcUaServerUrl, service, cancellationToken);

                return service;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task CleanupStaleServicesAsync()
        {
            //TODO: Needs implementation
            await Task.Yield();
        }

        private async Task AddServiceToDictionaryAsync(string endpointUrlKey, IAlarmSubscriptionService service, CancellationToken cancellationToken)
        {
            var serviceAddedToDictionary = false;
            while (!serviceAddedToDictionary)
            {
                cancellationToken.ThrowIfCancellationRequested();

                serviceAddedToDictionary = _subscriptionServices.TryAdd(endpointUrlKey, service);

                if (!serviceAddedToDictionary)
                    await Task.Delay(1000, cancellationToken);
            }
        }

        public void Dispose()
        {
            _subscriptionServices.Clear();
            _subscriptionServices = null;
            _semaphoreSlim?.Dispose();
        }
    }
}