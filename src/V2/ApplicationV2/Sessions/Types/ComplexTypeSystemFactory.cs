// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;


namespace ApplicationV2.Sessions.Types
{
    internal class ComplexTypeSystemFactory : IComplexTypeSystemFactory
    {
        public IComplexTypeSystem Create(Session session)
           => new ComplexTypeSystemWrapper(session!);
    }
}
