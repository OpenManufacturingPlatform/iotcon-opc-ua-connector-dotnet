// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Sessions.Types
{
    public interface IComplexTypeSystem
    {
        Task<bool> Load(bool onlyEnumTypes = false, bool throwOnError = false);
        Task<Type> LoadType(ExpandedNodeId nodeId, bool subTypes = false, bool throwOnError = false);
    }
}
