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
using OMP.Connector.Infrastructure.Kafka.Reconnect;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.Kafka.States
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
        private bool _disposed = false;

        public SessionPoolStateManager(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            ILoggerFactory loggerFactory,
            ApplicationConfiguration applicationConfiguration,
            IMapper mapper)
        {
            this._sessionPool = new ConcurrentDictionary<string, IOpcSession>();
            this._semaphoreSlim = new SemaphoreSlim(1);
            this._opcUaSettingOptions = connectorConfiguration;
            this._opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            this._loggerFactory = loggerFactory;
            this._applicationConfiguration = applicationConfiguration;
            this._mapper = mapper;
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
                await this._semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                opcUaServerUrl = opcUaServerUrl.ToValidBaseEndpointUrl();

                if (this._sessionPool.TryGetValue(opcUaServerUrl, out IOpcSession session)) { return session; }

                session = await this.OpenNewSessionAsync(opcUaServerUrl);
                await this.AddSessionToDictionaryAsync(opcUaServerUrl, session, cancellationToken);

                return session;
            }
            finally
            {
                this._semaphoreSlim.Release();
            }
        }

        private async Task AddSessionToDictionaryAsync(string key, IOpcSession session, CancellationToken cancellationToken)
        {
            var sessionAddedToDictionary = false;
            while (!sessionAddedToDictionary)
            {
                cancellationToken.ThrowIfCancellationRequested();

                sessionAddedToDictionary = this._sessionPool.TryAdd(key, session);

                if (!sessionAddedToDictionary)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task<IOpcSession> OpenNewSessionAsync(string opcUaServerUrl)
        {
            var session = new OpcSession(
                this._opcUaSettingOptions,
                this._opcSessionReconnectHandlerFactory,
                this._loggerFactory,
                this._applicationConfiguration,
                this._mapper);
            await session.ConnectAsync(opcUaServerUrl);
            return session;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (this._disposed) { return; }
            if (disposing)
            {
                foreach (var keyValue in this._sessionPool)
                {
                    keyValue.Value.Dispose();
                }
                this._sessionPool.Clear();
                this._semaphoreSlim?.Dispose();
            }
            this._disposed = true;
        }
    }
}