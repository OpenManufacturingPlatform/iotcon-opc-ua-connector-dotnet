// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Writes;

namespace ApplicationV2.Services
{
    public interface IWriteCommandService
    {
        Task<WriteResponseCollection> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken);
    }
}
