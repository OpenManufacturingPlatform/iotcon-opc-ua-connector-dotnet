// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Writes
{
    public record WriteCommand(WriteValue? Value, bool DoRegisteredWrite = false);
}
