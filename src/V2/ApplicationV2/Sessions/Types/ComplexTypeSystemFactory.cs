// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;


namespace OMP.PlantConnectivity.OpcUa.Sessions.Types
{
    internal sealed class ComplexTypeSystemFactory : IComplexTypeSystemFactory
    {
        public IComplexTypeSystem Create(Session session)
           => new ComplexTypeSystemWrapper(session!);
    }
}
