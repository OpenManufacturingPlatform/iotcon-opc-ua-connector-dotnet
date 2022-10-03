// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models
{
    public sealed record VariableNodeDataTypeInfo
    {
        public BuiltInType BuiltInType { get; set; }
        public Type? SystemDataType { get; set; }
        public int ValueRank { get; set; }
    }
}
