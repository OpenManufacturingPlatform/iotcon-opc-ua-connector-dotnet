// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Configuration
{
    public interface IAppConfigBuilder
    {
        ApplicationConfiguration Build();
    }
}
