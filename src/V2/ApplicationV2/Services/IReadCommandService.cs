// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Reads;
using ApplicationV2.Sessions;

namespace ApplicationV2.Services
{
    public interface IReadCommandService
    {
        Task<ReadResponseCollection> ReadValuesAsync(IOpcUaSession opcUaSession,ReadCommandCollection commands, CancellationToken cancellationToken);
    }
}
