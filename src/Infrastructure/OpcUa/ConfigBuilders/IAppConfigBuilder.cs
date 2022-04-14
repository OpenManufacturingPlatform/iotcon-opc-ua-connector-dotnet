// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ConfigBuilders
{
    public interface IAppConfigBuilder
    {
        ApplicationConfiguration Build();
    }
}