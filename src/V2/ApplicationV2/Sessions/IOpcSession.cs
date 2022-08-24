// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Configuration;
using ApplicationV2.Extensions;
using ApplicationV2.Sessions.Auth;
using ApplicationV2.Sessions.Reconnect;
using ApplicationV2.Sessions.RegisteredNodes;
using ApplicationV2.Sessions.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;
using EndpointConfiguration = Opc.Ua.EndpointConfiguration;

namespace ApplicationV2.Sessions
{

    public interface IOpcUaSession : IDisposable
    {
        Task ConnectAsync(string opcUaServerUrl);
        Task ConnectAsync(EndpointDescription endpointDescription);
        ResponseHeader WriteNodes(WriteValueCollection writeValues, out StatusCodeCollection statusCodeCollection);
        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);

        List<DataValue> ReadNodes(List<NodeId> nodeIds, int batchSize, out List<ServiceResult> errors);

        void RestoreRegisteredNodeIds();
        ResponseHeader RegisterNodes(NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
        ResponseHeader RegisterNodes(RequestHeader requestHeader, NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
    }

    public class OpcUaSession : IOpcUaSession
    {
        #region [Fields]
        private bool disposedValue;
        private SemaphoreSlim opcSessionSemaphore;
        private CancellationTokenSource sessionCancellationTokenSource;
        private Session? session;
        private readonly OpcUaConfiguration opcUaConfiguration;
        private readonly IRegisteredNodeStateManager registeredNodeStateManager;
        private readonly IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory;
        private readonly IUserIdentityProvider identityProvider;
        private readonly ApplicationConfiguration applicationConfiguration;
        private readonly IComplexTypeSystemFactory complexTypeSystemFactory;
        private IOpcUaSessionReconnectHandler? reconnectHandler;
        private readonly ILogger<OpcUaSession> logger;
        private readonly EndpointConfiguration endpointConfiguration;
        private IComplexTypeSystem? complexTypeSystem;
        private bool reconnectIsDisabled = false;
        #endregion

        #region [Ctor]
        public OpcUaSession(
            IOptions<OpcUaConfiguration> opcUaConfiguration,
            IRegisteredNodeStateManagerFactory registeredNodeStateManagerFactory,
            IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            IUserIdentityProvider identityProvider,
            ApplicationConfiguration applicationConfiguration,
            IComplexTypeSystemFactory complexTypeSystemFactory,
            ILogger<OpcUaSession> logger)
        {
            opcSessionSemaphore = new SemaphoreSlim(1);
            this.opcUaConfiguration = opcUaConfiguration.Value;
            sessionCancellationTokenSource = new CancellationTokenSource();
            this.registeredNodeStateManager = registeredNodeStateManagerFactory.Create(this, this.opcUaConfiguration.RegisterNodeBatchSize);
            this.opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            this.identityProvider = identityProvider;
            this.applicationConfiguration = applicationConfiguration;
            this.complexTypeSystemFactory = complexTypeSystemFactory;
            this.logger = logger;
            endpointConfiguration = EndpointConfiguration.Create(applicationConfiguration);
        }
        #endregion

        #region [Connect]
        public async Task ConnectAsync(string opcUaServerUrl)
        {
            var endpointDescriptionCollection = GetEndpoints(opcUaServerUrl);
            var endpointDescriptions = endpointDescriptionCollection.OrderByDescending(e => e.SecurityLevel);
            foreach (var endpointDescription in endpointDescriptions)
            {
                try
                {
                    await ConnectAsync(endpointDescription);
                    logger.LogInformation("Session created to Endpoint with: [{endpointUrl}] with SecurityMode: [{securityMode}] and Level: [{securityLevel}]", endpointDescription.EndpointUrl, endpointDescription.SecurityMode, endpointDescription.SecurityLevel);
                    break;
                }
                catch (Exception e)
                {
                    logger.LogWarning("Unable to create to Endpoint with: [{endpointUrl}] with SecurityMode: [{securityMode}] and Level: [{securityLevel}]", endpointDescription.EndpointUrl, endpointDescription.SecurityMode, endpointDescription.SecurityLevel);
                    await Task.Delay(500); // fix for Milo server not being happy with endpoint connections in quick succession
                }
            }

            if (session == default)
                throw new Exception($"Unable to create a session to OPC Server: [{endpointDescriptionCollection.FirstOrDefault()?.EndpointUrl}] on all its endpoints");
        }

        public async Task ConnectAsync(EndpointDescription endpointDescription)
        {
            var identity = identityProvider.GetUserIdentity(endpointDescription);
            await ConnectAsync(endpointDescription, identity);
        }
        #endregion

        #region [Register Nodes]
        public ResponseHeader RegisterNodes(NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds)
           => RegisterNodes(null, nodesToRegister, out registeredNodeIds);

        public ResponseHeader RegisterNodes(RequestHeader requestHeader, NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds)
        {
            CheckConnection();
            var result = session!.RegisterNodes(null, nodesToRegister, out registeredNodeIds);
            return result;
        }

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds)
            => registeredNodeStateManager.GetRegisteredNodeIds(nodeIds);


        public void RestoreRegisteredNodeIds()
        {
            registeredNodeStateManager.RestoreRegisteredNodeIds();
        }
        #endregion

        #region [Read]
        public List<DataValue> ReadNodes(List<NodeId> nodeIds, int batchSize, out List<ServiceResult> errors)
        {
            CheckConnection();

            var omitExpectedTypes = nodeIds.Select(_ => (Type)null).ToList();
            var values = new List<object>();
            errors = new List<ServiceResult>();

            var batchHandler = new BatchHandler<NodeId>(batchSize, ReadBatch(session!, values, errors, omitExpectedTypes!));
            batchHandler.RunBatches(nodeIds.ToList());

            logger.LogDebug("Executed Read commands. Endpoint: [{endpointUrl}]", session!.Endpoint.EndpointUrl);

            return values.Cast<DataValue>().ToList();//TODO: Test and Check with Hermo if this is valid and working
        }
        #endregion

        #region [Write]
        public ResponseHeader WriteNodes(WriteValueCollection writeValues, out StatusCodeCollection statusCodeCollection)
        {
            CheckConnection();
            var response = session!.Write(default, writeValues, out statusCodeCollection, out _);
            return response;
        } 
        #endregion


        #region [Disposal]
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        } 
        #endregion

        #region [Protected]
        protected virtual void Dispose(bool disposing)
        {

            if (disposedValue || !disposing) return;

            logger.LogTrace("Closing and disposing session: {endpointUrl}", session?.Endpoint?.EndpointUrl);

            sessionCancellationTokenSource?.Cancel();
            sessionCancellationTokenSource?.Dispose();

            reconnectHandler?.Dispose();

            opcSessionSemaphore?.Dispose();
            //opcSessionSemaphore = null;

            registeredNodeStateManager?.Dispose();

            complexTypeSystem = null;

            if (session is null)
                return;

            session.KeepAlive -= SessionOnKeepAlive;
            session.RemoveSubscriptions(session.Subscriptions.ToList());
            session.Close();
            session.Dispose();

            disposedValue = true;
        }
        #endregion

        #region [Privates]
        private EndpointDescriptionCollection GetEndpoints(string endpointAddress)
        {
            var uri = new Uri(endpointAddress);
            using var discoveryClient = DiscoveryClient.Create(uri, endpointConfiguration);
            var endpoints = discoveryClient.GetEndpoints(default);

            logger.LogDebug("Discovered [{endpointsCount}] endpoints for Server: [{endpointAddress}]", endpoints.Count, endpointAddress);
            return endpoints;
        }

        private void CheckConnection()
        {
            if (session is null)
                throw new Exception($"No open connection available. Please run {nameof(ConnectAsync)}");
        }

        private async Task<bool> LockSessionAsync()
        {
            logger.LogInformation("Locking session on Endpoint: [{endpoint}]", session?.Endpoint?.EndpointUrl);
            bool result;
            try
            {
                result = await opcSessionSemaphore.WaitAsync(TimeSpan.FromSeconds(opcUaConfiguration.AwaitSessionLockTimeoutSeconds), sessionCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void SessionOnKeepAlive(Session incommingSession, KeepAliveEventArgs e)
        {
            try
            {
                if (reconnectIsDisabled) return;

                if (incommingSession.SessionName != session!.SessionName)
                    return;

                if (!ReferenceEquals(incommingSession, session))
                    session = incommingSession;

                // start reconnect sequence on communication error.
                if (e != null && !ServiceResult.IsBad(e.Status))
                    return;

                var communicationError = $"Communication error: [{e?.Status}] on Endpoint: [{incommingSession.Endpoint?.EndpointUrl}]";

                if (opcUaConfiguration.ReconnectIntervalInSeconds <= 0)
                {
                    logger.LogWarning(communicationError);
                    logger.LogWarning("Reconnect is disabled. To enable reconnect, set the ReconnectIntervalInSeconds value to greater than 0");
                    reconnectIsDisabled = true;
                    return;
                }

                if (IsReconnectHandlerHealthy())
                    return;

                var lockObtained = LockSessionAsync().GetAwaiter().GetResult();
                if (!lockObtained) { return; }

                logger.LogWarning(communicationError);

                DisposeUnHealthyReconnectHandler();

                reconnectHandler = opcSessionReconnectHandlerFactory.Create();
                reconnectHandler.BeginReconnect(this, session, opcUaConfiguration.ReconnectIntervalInSeconds.ToMilliseconds(), ServerReconnectComplete!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private bool IsReconnectHandlerHealthy()
           => reconnectHandler is { IsHealthy: true };

        private void DisposeUnHealthyReconnectHandler()
        {
            if (reconnectHandler is null)
                return;

            if (!reconnectHandler.IsHealthy)
                reconnectHandler.Dispose();
        }

        private async Task LoadComplexTypeSystemAsync()
        {
            if (complexTypeSystem == null)
            {
                logger.LogTrace("Loading OPC UA complex type system...");
                var complexTypeSystemWrapper = complexTypeSystemFactory.Create(session!);
                complexTypeSystem = complexTypeSystemWrapper;
                await complexTypeSystemWrapper.Load();
                logger.LogTrace("Finished loading OPC UA complex type system.");
            }
        }

        private void ReleaseSession()
        {
            opcSessionSemaphore?.Release();
            logger.LogTrace("Releasing session on Endpoint: [{endpointUrl}]", session?.Endpoint?.EndpointUrl);
        }

        private void ServerReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                logger.LogInformation("Server successfully reconnected: {endpointUrl}", session?.Endpoint?.EndpointUrl);

                if (!ReferenceEquals(sender, reconnectHandler))
                    return;

                reconnectHandler?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                ReleaseSession();
            }
        }

        private async Task ConnectAsync(EndpointDescription endpointDescription, IUserIdentity identity)
        {
            try
            {
                var locked = await LockSessionAsync().ConfigureAwait(false);

                if (!locked) 
                    return;

                var sessionName = $"{applicationConfiguration.ApplicationUri}:{Guid.NewGuid()}";
                var endPointConfiguration = EndpointConfiguration.Create(applicationConfiguration);
                var configuredEndpoint = new ConfiguredEndpoint(endpointDescription.Server, endPointConfiguration);
                configuredEndpoint.Update(endpointDescription);

                session = await Session.Create(
                    applicationConfiguration,
                    configuredEndpoint,
                    true,
                    sessionName,
                    100000,
                    identity,
                    default);

                session.KeepAliveInterval = opcUaConfiguration.KeepAliveIntervalInSeconds.ToMilliseconds();
                session.KeepAlive += SessionOnKeepAlive;
                session.OperationTimeout = opcUaConfiguration.OperationTimeoutInSeconds.ToMilliseconds();

                await LoadComplexTypeSystemAsync();
            }
            finally
            {
                ReleaseSession();
            }
        }

        private Action<NodeId[]> ReadBatch(Session session, List<object> values, List<ServiceResult> errors, List<Type> omitExpectedTypes)
        {
            return (items) =>
            {
                logger.LogTrace("Reading {items} nodes...", items.Length);
                session.ReadValues(
                    items,
                    omitExpectedTypes.Take(items.Length).ToList(),
                    out var batchValues,
                    out var batchErrors);

                values.AddRange(batchValues);
                errors.AddRange(batchErrors);
            };
        }
        #endregion
    }
}
