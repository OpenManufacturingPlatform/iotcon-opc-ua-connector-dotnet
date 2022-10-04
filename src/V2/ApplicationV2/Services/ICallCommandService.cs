// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Call;
using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    public interface ICallCommandService
    {
        Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(IOpcUaSession opcUaSession, NodeMethodDescribeCommand command, CancellationToken cancellationToken);
        Task<CallCommandCollectionResponse> CallNodesAsync(IOpcUaSession opcUaSession, CallCommandCollection commands, CancellationToken cancellationToken);
    }
}
