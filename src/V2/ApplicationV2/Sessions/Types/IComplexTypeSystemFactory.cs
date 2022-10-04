// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;


namespace OMP.PlantConnectivity.OpcUa.Sessions.Types
{
    public interface IComplexTypeSystemFactory
    {
        IComplexTypeSystem Create(Session session);
    }
}
