// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.States
{
    public class SessionPoolStateManager : ISessionPoolStateManager
    {
        private readonly ConcurrentDictionary<string, IOpcSession> _sessionPool;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IOptions<ConnectorConfiguration> _opcUaSettingOptions;
        private readonly IOpcSessionReconnectHandlerFactory _opcSessionReconnectHandlerFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly IMapper _mapper;
        private readonly IUserIdentityProvider _identityProvider;
        private bool _disposed = false;

        public SessionPoolStateManager(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            ILoggerFactory loggerFactory,
            ApplicationConfiguration applicationConfiguration,
            IMapper mapper,
            IUserIdentityProvider identityProvider)
        {
            _sessionPool = new ConcurrentDictionary<string, IOpcSession>();
            _semaphoreSlim = new SemaphoreSlim(1);
            _opcUaSettingOptions = connectorConfiguration;
            _opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            _loggerFactory = loggerFactory;
            _applicationConfiguration = applicationConfiguration;
            _mapper = mapper;
            _identityProvider = identityProvider;
        }

        public Task CleanupStaleSessionsAsync()
        {
            //TODO: Implement once we have decided on a retention plan (expiration)
            throw new NotImplementedException();
        }

        public async Task<IOpcSession> GetSessionAsync(string opcUaServerUrl, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                opcUaServerUrl = opcUaServerUrl.ToValidBaseEndpointUrl();


                if (_sessionPool.TryGetValue(opcUaServerUrl, out IOpcSession session)) { return session; }

                session = await OpenNewSessionAsync(opcUaServerUrl);
                await AddSessionToDictionaryAsync(opcUaServerUrl, session, cancellationToken);

                return session;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task AddSessionToDictionaryAsync(string key, IOpcSession session, CancellationToken cancellationToken)
        {
            var sessionAddedToDictionary = false;
            while (!sessionAddedToDictionary)
            {
                cancellationToken.ThrowIfCancellationRequested();

                sessionAddedToDictionary = _sessionPool.TryAdd(key, session);

                if (!sessionAddedToDictionary)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task<IOpcSession> OpenNewSessionAsync(string opcUaServerUrl)
        {
            var session = new OpcSession(
                _opcUaSettingOptions,
                _opcSessionReconnectHandlerFactory,
                _loggerFactory,
                _applicationConfiguration,
                _mapper,
                _identityProvider);
            await session.ConnectAsync(opcUaServerUrl);
            return session;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) { return; }
            if (disposing)
            {
                foreach (var keyValue in _sessionPool)
                {
                    keyValue.Value.Dispose();
                }
                _sessionPool.Clear();
                _semaphoreSlim?.Dispose();
            }
            _disposed = true;
        }
    }
}