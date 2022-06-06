// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading.Tasks;
using Opc.Ua;

namespace OMP.Connector.Domain
{
    public interface IComplexTypeSystem
    {
        Task<Type> LoadType(ExpandedNodeId nodeId, bool subTypes = false, bool throwOnError = false);
    }
}
