// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Concurrent;
using OMP.PlantConnectivity.OpcUA.Configuration;
using OMP.PlantConnectivity.OpcUA.Extensions;
using OMP.PlantConnectivity.OpcUA.Services;
using OMP.PlantConnectivity.OpcUA.Sessions.Auth;
using OMP.PlantConnectivity.OpcUA.Sessions.Reconnect;
using OMP.PlantConnectivity.OpcUA.Sessions.RegisteredNodes;
using OMP.PlantConnectivity.OpcUA.Sessions.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Sessions.SessionManagement
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
        private readonly ILogger<SessionPoolStateManager> logger;

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
            this.logger = loggerFactory.CreateLogger<SessionPoolStateManager>();
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

        public async Task CloseAllSessionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Starting to close all active sessions");
                sessionPool.Values.ToList().ForEach(async opcsession =>
                {
                    var endpoint = opcsession.GetBaseEndpointUrl();
                    await opcsession.DisconnectAsync(cancellationToken);
                    logger.LogInformation("Sessions for {endpoin} closed", endpoint);
                });
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task CloseSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                sessionPool.Remove(opcUaServerUrl.ToValidBaseEndpointUrl(), out var session);
                await session.DisconnectAsync(cancellationToken);
                
                logger.LogInformation("Sessions for {endpoin} closed", opcUaServerUrl);
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
                    CloseAllSessionsAsync(CancellationToken.None).Wait();
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
