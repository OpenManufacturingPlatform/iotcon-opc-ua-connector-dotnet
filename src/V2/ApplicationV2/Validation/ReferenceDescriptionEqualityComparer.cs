// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Validation
{
    internal sealed class ReferenceDescriptionEqualityComparer : IEqualityComparer<ReferenceDescription>
    {
        public bool Equals(ReferenceDescription? x, ReferenceDescription? y)
        {
            return x?.NodeId == y?.NodeId;
        }

        public int GetHashCode(ReferenceDescription obj)
        {
            return obj.NodeId.GetHashCode();
        }
    }
}
