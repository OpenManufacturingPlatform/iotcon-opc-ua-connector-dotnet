// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Sessions.SessionManagement
{
    public interface ISessionPoolStateManager : IDisposable
    {
        Task<IOpcUaSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CloseSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CloseAllSessionsAsync(CancellationToken cancellationToken);

        Task CleanupStaleSessionsAsync();
    }
}
