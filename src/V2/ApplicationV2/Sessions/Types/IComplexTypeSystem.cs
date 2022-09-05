﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Sessions.Types
{
    public interface IComplexTypeSystem
    {
        Task<bool> Load(bool onlyEnumTypes = false, bool throwOnError = false);
        Task<Type> LoadType(ExpandedNodeId nodeId, bool subTypes = false, bool throwOnError = false);
    }
}