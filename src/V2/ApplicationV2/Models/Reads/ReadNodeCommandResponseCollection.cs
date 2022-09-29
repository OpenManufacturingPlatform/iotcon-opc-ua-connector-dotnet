// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Reads
{
    public class ReadNodeCommandResponseCollection : List<CommandResult<ReadNodeCommand, Node?>> { }
}
