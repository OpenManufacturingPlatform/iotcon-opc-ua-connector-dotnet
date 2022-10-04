// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Writes;
using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    public interface IWriteCommandService
    {
        Task<WriteResponseCollection> WriteAsync(IOpcUaSession opcUaSession, WriteCommandCollection commands, CancellationToken cancellationToken);
    }
}
