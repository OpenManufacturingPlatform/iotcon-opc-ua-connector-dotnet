// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    public interface ICallCommandService
    {
        Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(IOpcUaSession opcUaSession, NodeMethodDescribeCommand command, CancellationToken cancellationToken);
        Task<CallCommandCollectionResponse> CallNodesAsync(IOpcUaSession opcUaSession, CallCommandCollection commands, CancellationToken cancellationToken);
    }
}
