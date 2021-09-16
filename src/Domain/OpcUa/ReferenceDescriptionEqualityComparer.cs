using System.Collections.Generic;
using Opc.Ua;

namespace OMP.Connector.Domain.OpcUa
{
    public class ReferenceDescriptionEqualityComparer : IEqualityComparer<ReferenceDescription>
    {
        public bool Equals(ReferenceDescription x, ReferenceDescription y)
        {
            return x.NodeId == y.NodeId;
        }

        public int GetHashCode(ReferenceDescription obj)
        {
            return obj.NodeId.GetHashCode();
        }
    }
}