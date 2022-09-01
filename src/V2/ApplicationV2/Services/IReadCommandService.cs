// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Reads;
using ApplicationV2.Sessions;

namespace ApplicationV2.Services
{
    public interface IReadCommandService
    {
        Task<ReadValueResponseCollection> ReadValuesAsync(IOpcUaSession opcUaSession, ReadValueCommandCollection commands, CancellationToken cancellationToken);
        Task<ReadNodeCommandResponseCollection> ReadNodesAsync(IOpcUaSession opcUaSession, ReadNodeCommandCollection commands, CancellationToken cancellationToken);
    }
}
