using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IRegisteredNodeStateManager: IDisposable
    {
        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);

        public void RestoreRegisteredNodeIds(Session session);
    }
}