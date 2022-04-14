// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Infrastructure.OpcUa.States
{
    public class SubscriptionServiceStateManager : ISubscriptionServiceStateManager
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ISubscriptionServiceFactory _subscriptionServiceFactory;
        private ConcurrentDictionary<string, ISubscriptionService> _subscriptionServices;

        public SubscriptionServiceStateManager(ISubscriptionServiceFactory subscriptionServiceFactory)
        {
            _subscriptionServices = new ConcurrentDictionary<string, ISubscriptionService>();
            _semaphoreSlim = new SemaphoreSlim(1);
            _subscriptionServiceFactory = subscriptionServiceFactory;
        }

        public async Task<ISubscriptionService> GetSubscriptionServiceInstanceAsync(string opcUaServerUrl, CancellationToken cancellationToken)
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

        private async Task AddServiceToDictionaryAsync(string endpointUrlKey, ISubscriptionService service, CancellationToken cancellationToken)
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