// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading;

namespace ApplicationV2.Sessions.SessionManagement
{
    public interface ISessionPoolStateManager : IDisposable
    {
        Task<IOpcUaSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CleanupStaleSessionsAsync();
    }
}
