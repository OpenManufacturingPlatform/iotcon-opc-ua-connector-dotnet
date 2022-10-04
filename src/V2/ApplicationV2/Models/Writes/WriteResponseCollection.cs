// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Writes
{
    public class WriteResponseCollection : List<CommandResult<WriteCommand, StatusCode>> { }
}
