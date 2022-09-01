// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Call;
using ApplicationV2.Sessions;

namespace ApplicationV2.Services
{
    public interface ICallCommandService
    {
        Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(IOpcUaSession opcUaSession, NodeMethodDescribeCommand command, CancellationToken cancellationToken);
        Task<CallCommandCollectionResponse> CallNodesAsync(IOpcUaSession opcUaSession, CallCommandCollection commands, CancellationToken cancellationToken);
    }
}
