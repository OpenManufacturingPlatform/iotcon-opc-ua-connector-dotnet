// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest subscriptionRequests);
    }
}