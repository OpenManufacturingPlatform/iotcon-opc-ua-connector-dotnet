// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Serialization
{
    public interface IOmpOpcUaSerializer
    {
        string Serialize<T>(T value, string? fieldName = default);
        void ResetEncoder();
    }
}
