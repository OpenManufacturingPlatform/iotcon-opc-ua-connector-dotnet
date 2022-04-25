// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using Opc.Ua;

namespace OMP.Connector.Application.OpcUa
{
    public class OpcUaMethodInfo
    {
        public IEnumerable<Argument> InputArgs { get; set; }
        public IEnumerable<Argument> OutputArgs { get; set; }
        public NodeId MethodId { get; set; }
        public NodeId ObjectId { get; set; }
    }
}