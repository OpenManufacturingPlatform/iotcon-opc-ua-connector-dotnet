// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Serialization
{
    public interface IOmpOpcUaSerializerFactory
    {
        IOmpOpcUaSerializer Create(IOpcUaSession opcUaSession, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true);
    }
}
