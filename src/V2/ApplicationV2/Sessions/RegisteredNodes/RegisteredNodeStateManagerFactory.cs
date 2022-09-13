// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;

namespace OMP.PlantConnectivity.OpcUA.Sessions.RegisteredNodes
{
    internal sealed class RegisteredNodeStateManagerFactory : IRegisteredNodeStateManagerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public RegisteredNodeStateManagerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IRegisteredNodeStateManager Create(IOpcUaSession opcUaSession, int batchSize)
            => new RegisteredNodeStateManager(opcUaSession, batchSize, loggerFactory.CreateLogger<RegisteredNodeStateManager>());
    }
}
