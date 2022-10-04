// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Configuration
{
    public interface IAppConfigBuilder
    {
        ApplicationConfiguration Build();
    }
}
