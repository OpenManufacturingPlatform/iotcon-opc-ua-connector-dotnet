﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Interfaces;
using Opc.Ua.Client;

namespace OMP.Connector.Domain.Providers
{
    public interface ISubscriptionProvider
    {
        Task<ICommandResponse> ExecuteAsync(Session session, IComplexTypeSystem complexTypeSystem);
    }
}
