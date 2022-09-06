// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Sessions.RegisteredNodes
{
    public interface IRegisteredNodeStateManagerFactory
    {
        IRegisteredNodeStateManager Create(IOpcUaSession opcUaSession, int batchSiz);
    }
}
