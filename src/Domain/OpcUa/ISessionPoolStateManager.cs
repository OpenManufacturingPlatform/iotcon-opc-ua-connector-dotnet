using System;
using System.Threading;
using System.Threading.Tasks;

namespace OMP.Connector.Domain.OpcUa
{
    public interface ISessionPoolStateManager: IDisposable
    {
        Task<IOpcSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CleanupStaleSessionsAsync();
    }
}
