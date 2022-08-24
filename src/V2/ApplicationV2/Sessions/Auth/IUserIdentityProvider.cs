// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Sessions.Auth
{
    public interface IUserIdentityProvider
    {
        IUserIdentity GetUserIdentity(EndpointDescription endpointDescription);
    }

}
