// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Sessions.RegisteredNodes
{
    public interface IRegisteredNodeStateManager : IDisposable
    {
        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);
        void RestoreRegisteredNodeIds();
    }
}
