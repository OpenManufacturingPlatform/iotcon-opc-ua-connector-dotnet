// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Reads;

namespace ApplicationV2.Services
{
    public interface IReadCommandService
    {
        Task<ReadResponseCollection> ReadValuesAsync(ReadCommandCollection commands, CancellationToken cancellationToken);
    }
}
