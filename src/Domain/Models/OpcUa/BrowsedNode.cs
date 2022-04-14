// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa
{
    public class BrowsedNode
    {
        public Node Node { get; set; }

        public List<BrowsedNode> ChildNodes { get; set; } = new List<BrowsedNode>();
    }
}
