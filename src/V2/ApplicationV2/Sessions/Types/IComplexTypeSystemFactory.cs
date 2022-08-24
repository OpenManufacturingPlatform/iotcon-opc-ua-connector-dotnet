// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;


namespace ApplicationV2.Sessions.Types
{
    public interface IComplexTypeSystemFactory
    {
        IComplexTypeSystem Create(Session session);
    }
}
