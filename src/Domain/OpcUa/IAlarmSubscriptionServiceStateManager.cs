// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IAlarmSubscriptionServiceStateManager : IDisposable
    {
        Task<IAlarmSubscriptionService> GetAlarmSubscriptionServiceInstanceAsync(string opcUaServerUrl, CancellationToken cancellationToken);

        Task CleanupStaleServicesAsync();
    }
}
