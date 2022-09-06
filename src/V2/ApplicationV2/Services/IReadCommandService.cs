// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Reads;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    public interface IReadCommandService
    {
        Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcUaSession, ReadValueCommandCollection commands, CancellationToken cancellationToken);
        Task<ReadNodeCommandResponseCollection> ReadNodesAsync(IOpcUaSession opcUaSession, ReadNodeCommandCollection commands, CancellationToken cancellationToken);
    }
}
