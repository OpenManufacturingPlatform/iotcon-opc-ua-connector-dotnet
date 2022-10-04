// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Sessions.SessionManagement
{
    public interface ISessionPoolStateManager : IDisposable
    {
        Task<IOpcUaSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CloseSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CloseAllSessionsAsync(CancellationToken cancellationToken);

        Task CleanupStaleSessionsAsync();
    }
}
