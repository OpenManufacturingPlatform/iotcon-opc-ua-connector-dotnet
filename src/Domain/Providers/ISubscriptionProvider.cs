// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Providers
{
    public interface ISubscriptionProvider
    {
        Task<ICommandResponse> ExecuteAsync(IOpcSession opcSession);
    }
}
