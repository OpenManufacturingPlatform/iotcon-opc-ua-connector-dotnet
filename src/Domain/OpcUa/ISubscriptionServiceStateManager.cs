using System;
using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Domain.OpcUa
{
    public interface ISubscriptionServiceStateManager : IDisposable
    {
        Task<ISubscriptionService> GetSubscriptionServiceInstanceAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CleanupStaleServicesAsync();
    }
}