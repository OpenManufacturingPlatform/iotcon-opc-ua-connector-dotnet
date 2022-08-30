// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Configuration;
using ApplicationV2.Extensions;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Services;
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
        #region [Misc]
        string GetBaseEndpointUrl(); 
        #endregion

        #region [Connection]
        Task ConnectAsync(string opcUaServerUrl);
        Task ConnectAsync(EndpointDescription endpointDescription);
        #endregion

        #region [Write]
        ResponseHeader WriteNodes(WriteValueCollection writeValues, out StatusCodeCollection statusCodeCollection);
        #endregion

        #region [Read]
        List<object> ReadNodes(List<NodeId> nodeIds, int batchSize, out List<ServiceResult> errors);
        #endregion

        #region [Registered Nodes]
        void RestoreRegisteredNodeIds();
        ResponseHeader RegisterNodes(NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
        ResponseHeader RegisterNodes(RequestHeader requestHeader, NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);
        #endregion

        #region [Subscriptions]
        Subscription CreateOrUpdateSubscription(SubscriptionMonitoredItem monitoredItem, bool autoApplyChanges = false);
        void ActivatePublishingOnAllSubscriptions();

        IEnumerable<Subscription> GetSubscriptions();

        Task<bool> RemoveSubscriptionAsync(Subscription subscription);
        #endregion
    }

    public class OpcUaSession : IOpcUaSession
    {
        #region [Fields]
        private bool disposedValue;
        private SemaphoreSlim opcSessionSemaphore;
        private CancellationTokenSource sessionCancellationTokenSource;
        private Session? session;
        private readonly OmpOpcUaConfiguration opcUaConfiguration;
        private readonly IRegisteredNodeStateManager registeredNodeStateManager;
        private readonly IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory;
        private readonly IUserIdentityProvider identityProvider;
        private readonly ApplicationConfiguration applicationConfiguration;
        private readonly IComplexTypeSystemFactory complexTypeSystemFactory;
        private readonly IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors;
        private IOpcUaSessionReconnectHandler? reconnectHandler;
        private readonly ILogger<OpcUaSession> logger;
        private readonly EndpointConfiguration endpointConfiguration;
        private IComplexTypeSystem? complexTypeSystem;
        private bool reconnectIsDisabled = false;
        #endregion

        #region [Ctor]
        public OpcUaSession(
            IOptions<OmpOpcUaConfiguration> opcUaConfiguration,
            IRegisteredNodeStateManagerFactory registeredNodeStateManagerFactory,
            IOpcUaSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            IUserIdentityProvider identityProvider,
            ApplicationConfiguration applicationConfiguration,
            IComplexTypeSystemFactory complexTypeSystemFactory,
            IEnumerable<IMonitoredItemMessageProcessor> monitoredItemMessageProcessors,
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
            this.monitoredItemMessageProcessors = monitoredItemMessageProcessors;
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
        public List<object> ReadNodes(List<NodeId> nodeIds, int batchSize, out List<ServiceResult> errors)
        {
            CheckConnection();

            var omitExpectedTypes = nodeIds.Select(_ => (Type)null).ToList();
            var values = new List<object>();
            errors = new List<ServiceResult>();

            var batchHandler = new BatchHandler<NodeId>(batchSize, ReadBatch(session!, values, errors, omitExpectedTypes!));
            batchHandler.RunBatches(nodeIds.ToList());

            logger.LogDebug("Executed Read commands. Endpoint: [{endpointUrl}]", session!.Endpoint.EndpointUrl);

            return values.ToList();//TODO: Test and Check with Hermo if this is valid and working
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

        #region [Subscriptions] 

        public Subscription CreateOrUpdateSubscription(SubscriptionMonitoredItem monitoredItem, bool autoApplyChanges = false)
        {
            CheckConnection();
            var opcUaSubscription = this.GetSubscription(monitoredItem);

            var subscription = opcUaSubscription == default
                            ? this.CreateNewSubscription(monitoredItem)
                            : this.ModifySubscription(opcUaSubscription, monitoredItem);

            if (autoApplyChanges)
                subscription.ApplyChanges();

            return subscription;
        }

        public void ActivatePublishingOnAllSubscriptions()
        {
            CheckConnection();

            foreach (var sub in session!.Subscriptions.Where(sub => !sub.PublishingEnabled))
            {
                logger.LogTrace($"Enabling publishing for subscription {sub.Id}");
                sub.SetPublishingMode(true);
            }
        }

        public IEnumerable<Subscription> GetSubscriptions()
        {
            CheckConnection();
            return session!.Subscriptions;
        }

        public Task<bool> RemoveSubscriptionAsync(Subscription subscription)
        {
            CheckConnection();
            return session!.RemoveSubscriptionAsync(subscription);
        }

        #endregion

        #region [Disposal]
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region [Misc]
        public string GetBaseEndpointUrl()
            => session?.GetBaseEndpointUrl() ?? string.Empty;
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

        #region [Subscriptions]
        private Subscription? GetSubscription(SubscriptionMonitoredItem monitoredItem)
        {
            if (monitoredItem == default)
                return null;

            var subscriptions = session!.Subscriptions
                .Where(x => x.MonitoredItems.Any(y => monitoredItem.NodeId.Equals(y.ResolvedNodeId.ToString())));

            return subscriptions.FirstOrDefault();
        }

        private Subscription CreateNewSubscription(SubscriptionMonitoredItem monitoredItem)
        {
            var keepAliveCount = Convert.ToUInt32(monitoredItem.HeartbeatInterval);
            var subscription = session!.Subscriptions.FirstOrDefault(x => monitoredItem.PublishingInterval.Equals(x.PublishingInterval));
            if (subscription == default)
            {
                subscription = new Subscription
                {
                    PublishingInterval = monitoredItem.PublishingInterval,
                    LifetimeCount = 100000,
                    KeepAliveCount = keepAliveCount > 0 ? keepAliveCount : 100000,
                    MaxNotificationsPerPublish = 1,
                    Priority = 0,
                    PublishingEnabled = false
                };

                session!.AddSubscription(subscription);
                subscription.Create();
            }
            var item = this.CreateMonitoredItem(monitoredItem);
            subscription.AddItem(item);
            return subscription;
        }

        private Subscription ModifySubscription(Subscription opcUaSubscription, SubscriptionMonitoredItem monitoredItem)
        {
            var existingItems = opcUaSubscription
                .MonitoredItems
                .Where(x => monitoredItem.NodeId.Equals(x.ResolvedNodeId.ToString()))
                .ToList();

            if (SamplingIntervalsAreTheSame(monitoredItem, existingItems))
                return opcUaSubscription;

            opcUaSubscription.RemoveItems(existingItems);// Notification of intent
            opcUaSubscription.ApplyChanges(); // enforces intent is executed
            return this.CreateNewSubscription(monitoredItem); // now re-add the monitored item
        }

        private MonitoredItem CreateMonitoredItem(SubscriptionMonitoredItem subscriptionMonitoredItem)
        {
            try
            {
                var monitoredItem = new MonitoredItem
                {
                    StartNodeId = subscriptionMonitoredItem.NodeId,
                    AttributeId = subscriptionMonitoredItem.AttributeId,
                    MonitoringMode = subscriptionMonitoredItem.MonitoringMode,
                    SamplingInterval = subscriptionMonitoredItem.SamplingInterval,
                    QueueSize = subscriptionMonitoredItem.QueueSize,
                    DiscardOldest = subscriptionMonitoredItem.DiscardOldest
                };

                foreach (var processor in monitoredItemMessageProcessors)
                    monitoredItem.Notification += processor.ProcessMessage; //If processor throws error the application crashes

                logger.LogTrace("Monitored item with NodeId: {nodeId}, Sampling Interval: [{samplingInterval}] has been created successfully"
                    , subscriptionMonitoredItem.NodeId
                    , monitoredItem.SamplingInterval);

                return monitoredItem;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unable to create monitored item with NodeId: [{nodeId}] | Error: {error}", subscriptionMonitoredItem.NodeId, ex);
                //TODO: subscribe to all items we can but let the error buble up as well
                throw;
            }
        }

        private static bool SamplingIntervalsAreTheSame(SubscriptionMonitoredItem monitoredItem, List<MonitoredItem> existingItems)
            => existingItems.Any(m => m.SamplingInterval == monitoredItem.SamplingInterval);
        #endregion
        #endregion
    }
}
