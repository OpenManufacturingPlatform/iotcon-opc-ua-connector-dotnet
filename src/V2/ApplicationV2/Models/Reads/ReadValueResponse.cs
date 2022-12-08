// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Reads
{
    public record ReadValueResponse(object Value, ServiceResult ServiceResult);
}
