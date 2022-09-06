// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Writes;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    public interface IWriteCommandService
    {
        Task<WriteResponseCollection> WriteAsync(IOpcUaSession opcUaSession, WriteCommandCollection commands, CancellationToken cancellationToken);
    }
}
