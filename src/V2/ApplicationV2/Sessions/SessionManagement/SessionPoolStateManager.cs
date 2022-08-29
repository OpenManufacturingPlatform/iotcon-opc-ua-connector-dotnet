// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Concurrent;
using ApplicationV2.Configuration;
using ApplicationV2.Extensions;
using ApplicationV2.Services;
using ApplicationV2.Sessions.Auth;
using ApplicationV2.Sessions.Reconnect;
using ApplicationV2.Sessions.RegisteredNodes;
using ApplicationV2.Sessions.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;

namespace ApplicationV2.Sessions.SessionManagement
{
    internal class SessionPoolStateManager : ISessionPoolStateManager
    {
        private bool disposedValue;

        private readonly ConcurrentDictionary<string, IOpcUaSession> sessionPool;
        private readonly SemaphoreSlim semaphoreSlim;
        private readonly IOptions<OmpOpcUaConfiguration> opcUaConfiguration;
        private readonly IRegisteredNodeStateManagerFactory registeredNodeStateManagerFactory;
        private readonly IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory;
        private readonly IUserIdentityProvider identityProvider;
        private readonly ApplicationConfiguration applicationConfiguration;
        private readonly IComplexTypeSystemFactory complexTypeSystemFactory;
        private readonly IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors;
        private readonly ILoggerFactory loggerFactory;

        public SessionPoolStateManager(
            IOptions<OmpOpcUaConfiguration> opcUaConfiguration,
            IRegisteredNodeStateManagerFactory registeredNodeStateManagerFactory,
            IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            IUserIdentityProvider identityProvider,
            ApplicationConfiguration applicationConfiguration,
            IComplexTypeSystemFactory complexTypeSystemFactory,
            IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors,
            ILoggerFactory loggerFactory)
        {
            sessionPool = new ConcurrentDictionary<string, IOpcUaSession>();
            semaphoreSlim = new SemaphoreSlim(1);
            this.opcUaConfiguration = opcUaConfiguration;
            this.registeredNodeStateManagerFactory = registeredNodeStateManagerFactory;
            this.opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            this.identityProvider = identityProvider;
            this.applicationConfiguration = applicationConfiguration;
            this.complexTypeSystemFactory = complexTypeSystemFactory;
            this.monitoredItemMessageProcessors = monitoredItemMessageProcessors;
            this.loggerFactory = loggerFactory;
        }

        public Task CleanupStaleSessionsAsync()
        {
            //TODO: Implement once we have decided on a retention plan (expiration)
            throw new NotImplementedException();
        }

        public async Task<IOpcUaSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                opcUaServerUrl = opcUaServerUrl.ToValidBaseEndpointUrl();


                if (sessionPool.TryGetValue(opcUaServerUrl, out var session))
                    return session;

                session = await OpenNewSessionAsync(opcUaServerUrl);
                await AddSessionToDictionaryAsync(opcUaServerUrl, session, cancellationToken);

                return session;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<IOpcUaSession> OpenNewSessionAsync(string opcUaServerUrl)
        {
            var session = new OpcUaSession(
                opcUaConfiguration,
                registeredNodeStateManagerFactory,
                opcSessionReconnectHandlerFactory,
                identityProvider,
                applicationConfiguration,
                complexTypeSystemFactory,
                monitoredItemMessageProcessors,
                loggerFactory.CreateLogger<OpcUaSession>());

            await session.ConnectAsync(opcUaServerUrl);
            return session;
        }

        private async Task AddSessionToDictionaryAsync(string key, IOpcUaSession session, CancellationToken cancellationToken)
        {
            var sessionAddedToDictionary = false;
            while (!sessionAddedToDictionary)
            {
                cancellationToken.ThrowIfCancellationRequested();

                sessionAddedToDictionary = sessionPool.TryAdd(key, session);

                if (!sessionAddedToDictionary)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
