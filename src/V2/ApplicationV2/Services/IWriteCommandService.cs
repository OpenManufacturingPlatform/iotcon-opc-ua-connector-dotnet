// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Writes;
using ApplicationV2.Sessions;

namespace ApplicationV2.Services
{
    public interface IWriteCommandService
    {
        Task<WriteResponseCollection> WriteAsync(IOpcUaSession opcUaSession, WriteCommandCollection commands, CancellationToken cancellationToken);
    }
}
