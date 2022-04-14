﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa
{
    public class VariableTypeNodeWithType : VariableTypeNode
    {
        public string DataTypeName { get; set; }
    }
}