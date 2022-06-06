// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionRestoreService
    {
        Task RestoreSubscriptionsAsync(IOpcSession opcSession);
    }
}