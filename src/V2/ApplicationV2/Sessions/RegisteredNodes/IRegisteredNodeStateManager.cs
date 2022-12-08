// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Sessions.RegisteredNodes
{
    public interface IRegisteredNodeStateManager : IDisposable
    {
        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);
        void RestoreRegisteredNodeIds();
    }
}
