// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IUserIdentityProvider
    {
        IUserIdentity GetUserIdentity(EndpointDescription endpointDescription);
    }
}