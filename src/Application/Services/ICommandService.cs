// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Application.Services
{
    public interface ICommandService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest);
    }
}