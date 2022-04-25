// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa
{
    public class VariableNodeWithType : VariableNode
    {
        public string DataTypeName { get; set; }
    }
}
