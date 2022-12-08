// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Reads;
using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    public interface IReadCommandService
    {
        Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcUaSession, ReadValueCommandCollection commands, CancellationToken cancellationToken);
        Task<ReadNodeCommandResponseCollection> ReadNodesAsync(IOpcUaSession opcUaSession, ReadNodeCommandCollection commands, CancellationToken cancellationToken);
    }
}
