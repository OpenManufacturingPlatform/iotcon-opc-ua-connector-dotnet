// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionServiceFactory
    {
        ISubscriptionService Create();
    }
}