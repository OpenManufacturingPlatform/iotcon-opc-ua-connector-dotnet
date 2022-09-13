// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Serialization
{
    public interface IOmpOpcUaSerializerFactory
    {
        IOmpOpcUaSerializer Create(IOpcUaSession opcUaSession, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true);
    }
}
