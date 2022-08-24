// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Sessions.RegisteredNodes
{
    public interface IRegisteredNodeStateManagerFactory
    {
        IRegisteredNodeStateManager Create(IOpcUaSession opcUaSession, int batchSiz);
    }
}
