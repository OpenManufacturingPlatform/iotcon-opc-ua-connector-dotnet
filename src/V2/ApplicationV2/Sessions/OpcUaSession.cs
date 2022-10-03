// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections;
using System.Diagnostics;
using OMP.PlantConnectivity.OpcUA.Configuration;
using OMP.PlantConnectivity.OpcUA.Extensions;
using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Sessions.Auth;
using OMP.PlantConnectivity.OpcUA.Sessions.Reconnect;
using OMP.PlantConnectivity.OpcUA.Sessions.RegisteredNodes;
using OMP.PlantConnectivity.OpcUA.Sessions.Types;
using OMP.PlantConnectivity.OpcUA.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;
using EndpointConfiguration = Opc.Ua.EndpointConfiguration;
using TypeInfo = Opc.Ua.TypeInfo;
using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Services.Alarms;
using OMP.PlantConnectivity.OpcUA.Services.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Models;

namespace OMP.PlantConnectivity.OpcUA.Sessions
{
    internal class OpcUaSession : IOpcUaSession
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
        private readonly IEnumerable<IAlarmMonitoredItemMessageProcessor> alarmMonitoredItemMessageProcessors;
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
            IEnumerable<IAlarmMonitoredItemMessageProcessor> alarmMonitoredItemMessageProcessors,
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
            this.alarmMonitoredItemMessageProcessors = alarmMonitoredItemMessageProcessors;
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
                    logger.LogInformation("Session created to endpoint [{endpointUrl}] with SecurityMode: [{securityMode}] and Level: [{securityLevel}]", endpointDescription.EndpointUrl, endpointDescription.SecurityMode, endpointDescription.SecurityLevel);
                    break;
                }
                catch (Exception)
                {
                    logger.LogWarning("Unable to create to endpoint [{endpointUrl}] with SecurityMode: [{securityMode}] and Level: [{securityLevel}]", endpointDescription.EndpointUrl, endpointDescription.SecurityMode, endpointDescription.SecurityLevel);
                    await Task.Delay(500); // fix for Milo server not being happy with endpoint connections in quick succession
                }
            }

            if (session == default)
                throw new Exception($"Unable to create a session to OPC UA Server: [{endpointDescriptionCollection.FirstOrDefault()?.EndpointUrl}] on all its endpoints");
        }

        public async Task ConnectAsync(EndpointDescription endpointDescription)
        {
            var identity = identityProvider.GetUserIdentity(endpointDescription);
            await ConnectAsync(endpointDescription, identity);
        }

        public async Task DisconnectAsync(CancellationToken stoppingToken)
        {
            try
            {
                session!.KeepAlive -= SessionOnKeepAlive;
                if (session.Connected && !session.KeepAliveStopped)
                {
                    await session.CloseSessionAsync(null, true, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                throw ex.Demystify();
            }
        }
        #endregion

        #region [Browse]
        public Task<ReferenceDescriptionCollection> BrowseAsync(BrowseDescription browseDescription, CancellationToken? cancellationToken = null)
            => BrowseAsync(session!, browseDescription, logger, cancellationToken);

        #endregion

        #region [Call]

        public async Task<IEnumerable<NodeMethodDescribeResponse>> GetMethodInfoListAsync(IEnumerable<NodeId> nodeIds, CancellationToken cancellationToken)
        {
            CheckConnection();

            var listMethodInfo = new List<NodeMethodDescribeResponse>();
            foreach (var nodeId in nodeIds)
            {
                var response = await GetNodeMethodArgumentsAsync(nodeId, cancellationToken);
                listMethodInfo.Add(response);
            }

            return listMethodInfo;
        }

        public Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(NodeMethodDescribeCommand command, CancellationToken cancellationToken)
            => GetNodeMethodArgumentsAsync(command.NodeId, cancellationToken);

        public async Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(NodeId nodeId, CancellationToken cancellationToken)
        {
            CheckConnection();
            var response = new NodeMethodDescribeResponse { MethodId = nodeId };
            try
            {
                var inputArguments = new List<Argument>();
                var outArguments = new List<Argument>();
                var browseDescription = new BrowseDescription
                {
                    NodeId = nodeId,
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = ReferenceTypeIds.HasProperty,
                    IncludeSubtypes = true,
                    NodeClassMask = (uint)NodeClass.Variable,
                    ResultMask = (uint)BrowseResultMask.BrowseName
                };

                var methodReferences = await BrowseAsync(session!, browseDescription, logger, cancellationToken);

                var readValuesIds = (from reference in methodReferences
                                     where !reference.NodeId.IsAbsolute
                                     where reference.BrowseName == BrowseNames.InputArguments
                                           || reference.BrowseName == BrowseNames.OutputArguments
                                     select new ReadValueId { NodeId = (NodeId)reference.NodeId, AttributeId = Attributes.Value, Handle = reference }
                                    )
                                    .ToList();

                if (!readValuesIds.Any())
                    return response;

                var readValueIdCollection = new ReadValueIdCollection(readValuesIds);

                var readNodeResult = await session!.ReadAsync(default, 0, TimestampsToReturn.Neither, readValueIdCollection, cancellationToken);

                ValidateResponseDiagnostics(readValueIdCollection, readNodeResult.Results, readNodeResult.DiagnosticInfos);

                var combinedValueTuples = readValuesIds.Zip(readNodeResult.Results);

                foreach (var (readValue, dataValue) in combinedValueTuples)
                {
                    if (!StatusCode.IsGood(dataValue.StatusCode)) continue;

                    var reference = (ReferenceDescription)readValue.Handle;

                    if (reference.BrowseName == BrowseNames.InputArguments)
                        inputArguments.AddRange((Argument[])ExtensionObject.ToArray(dataValue.GetValue<ExtensionObject[]>(Array.Empty<ExtensionObject>()), typeof(Argument)));


                    if (reference.BrowseName == BrowseNames.OutputArguments)
                        outArguments.AddRange((Argument[])ExtensionObject.ToArray(dataValue.GetValue<ExtensionObject[]>(Array.Empty<ExtensionObject>()), typeof(Argument)));
                }

                foreach (var inputArgument in inputArguments)
                    inputArgument.Value = TypeInfo.GetDefaultValue(inputArgument.DataType, inputArgument.ValueRank, session.TypeTree);

                response = new NodeMethodDescribeResponse
                {
                    MethodId = nodeId,
                    InputArguments = inputArguments,
                    OutArguments = outArguments
                };

                return response;

            }
            finally
            {
                var node = session!.NodeCache.Find(nodeId, ReferenceTypes.HasComponent, true, false).FirstOrDefault();
                if (node is not null)
                    response.ObjectId = ExpandedNodeId.ToNodeId(node.NodeId, session.NodeCache.NamespaceUris);
            }
        }

        public Task<CallResponse> CallAsync(IEnumerable<CallMethodRequest> callMethodRequests, CancellationToken? cancellationToken = null)
        {
            var callMethodRequestCollection = new CallMethodRequestCollection(callMethodRequests);
            return CallAsync(callMethodRequestCollection, cancellationToken);
        }

        public Task<CallResponse> CallAsync(CallMethodRequestCollection callMethodRequestCollection, CancellationToken? cancellationToken = null)
        {
            if (!callMethodRequestCollection.Any())
                return Task.FromResult(new CallResponse()); //TODO: Validate if this is the right behaviour

            CheckConnection();
            var stoppingToken = cancellationToken ?? CancellationToken.None;
            return session!.CallAsync(default, callMethodRequestCollection, stoppingToken);
        }
        #endregion

        #region [Register Nodes]
        public ResponseHeader RegisterNodes(NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds)
           => RegisterNodes(null, nodesToRegister, out registeredNodeIds);

        public ResponseHeader RegisterNodes(RequestHeader? requestHeader, NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds)
        {
            CheckConnection();
            var result = session!.RegisterNodes(requestHeader, nodesToRegister, out registeredNodeIds);
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

        public Node? ReadNode(NodeId nodeId)
        {
            CheckConnection();
            return session!.NodeCache.FetchNode(nodeId) ?? session!.ReadNode(nodeId);
        }

        public List<object> ReadNodeValues(List<NodeId> nodeIds, out List<ServiceResult> errors)
        {
            CheckConnection();

            var ignoreCheckForExpectedTypes = new Type[nodeIds.Count];

            session!.ReadValues(
                nodeIds,
                ignoreCheckForExpectedTypes.ToList(),
                out var values,
                out errors);

            return values;
        }

        public string GetNodeFriendlyDataType(NodeId dataTypeNodeId, int valueRank)
        {
            CheckConnection();
            return GetNodeFriendlyDataType(session!, dataTypeNodeId, valueRank);
        }
        #endregion

        #region [Write]
        public ResponseHeader WriteNodes(WriteValueCollection writeValues, out StatusCodeCollection statusCodeCollection)
        {
            CheckConnection();
            var response = session!.Write(default, writeValues, out statusCodeCollection, out _);
            return response;
        }

        public async Task<VariableNodeDataTypeInfo> GetVariableNodeDataTypeInfoAsync(NodeId variableNodeId)
        {
            var nodeDataTypeAttribute = await session!.ReadAsync(
                default,
                default,
                default,
                GetDataTypeAndValueRankQuery(variableNodeId),
                CancellationToken.None);

            var nodeDataTypeId = nodeDataTypeAttribute.Results[0].Value as NodeId;
            var valueRank = (int)nodeDataTypeAttribute.Results[1].Value;
            var builtInType = GetBuiltInType(nodeDataTypeId!);
            var systemType = builtInType != BuiltInType.ExtensionObject
                ? TypeInfo.GetSystemType(builtInType, -1) //always retrieve scalar type instead of array type when valueRank > -1
                : await complexTypeSystem!.LoadType(nodeDataTypeId);

            return new VariableNodeDataTypeInfo()
            {
                BuiltInType = builtInType,
                SystemDataType = systemType,
                ValueRank = valueRank
            };
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

        public void RefreshAlarmsOnAllSubscriptions()
        {
            CheckConnection();

            foreach (var sub in session!.Subscriptions.Where(sub => !sub.PublishingEnabled))
            {
                logger.LogTrace($"Refreshing alarms for subscription {sub.Id}");
                sub.ConditionRefresh();
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
        public Task<bool> RemoveSubscriptionsAsync(IEnumerable<Subscription> subscriptions)
        {
            CheckConnection();
            return session!.RemoveSubscriptionsAsync(subscriptions);
        }

        #endregion

        #region [Alarms]
        public Subscription CreateOrUpdateAlarmSubscription(AlarmSubscriptionMonitoredItem alarmMonitoredItem, bool autoApplyChanges = false)
        {
            CheckConnection();
            var opcUaSubscription = this.GetAlarmSubscription(alarmMonitoredItem);

            var subscription = opcUaSubscription == default
                            ? this.CreateNewAlarmSubscription(alarmMonitoredItem)
                            : this.ModifyAlarmSubscription(opcUaSubscription, alarmMonitoredItem);

            if (autoApplyChanges)
                subscription.ApplyChanges();

            return subscription;
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

        public NamespaceTable? GetNamespaceUris()
            => session?.NamespaceUris;

        public IServiceMessageContext? GetServiceMessageContext()
            => session?.MessageContext;
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

        #region [Connection]
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
                    opcUaConfiguration.SessionTimeoutInMs,
                    identity,
                    default);

                session.KeepAliveInterval = opcUaConfiguration.SessionKeepAliveIntervalInSeconds.ToMilliseconds();
                session.KeepAlive += SessionOnKeepAlive;
                session.OperationTimeout = opcUaConfiguration.OperationTimeoutInSeconds.ToMilliseconds();

                await LoadComplexTypeSystemAsync();
            }
            finally
            {
                ReleaseSession();
            }
        }
        #endregion

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
            var subscription = session!.Subscriptions.FirstOrDefault(x => monitoredItem.PublishingInterval.Equals(x.PublishingInterval));
            if (subscription == default)
            {
                subscription = new Subscription
                {
                    PublishingInterval = monitoredItem.PublishingInterval,
                    LifetimeCount = opcUaConfiguration.SubscriptionLifetimeCountInMs,
                    KeepAliveCount = monitoredItem.KeepAliveCount > 0 ? monitoredItem.KeepAliveCount : opcUaConfiguration.SubscriptionKeepAliveCountInMs,
                    MaxNotificationsPerPublish = 1,
                    Priority = monitoredItem.Priority,
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
                //TODO: subscribe to all items we can but let the error bubble up as well
                throw;
            }
        }

        private static bool SamplingIntervalsAreTheSame(SubscriptionMonitoredItem monitoredItem, List<MonitoredItem> existingItems)
            => existingItems.Any(m => m.SamplingInterval == monitoredItem.SamplingInterval);
        #endregion

        #region [Alarms]
        private Subscription? GetAlarmSubscription(AlarmSubscriptionMonitoredItem alarmMonitoredItem)
        {
            if (alarmMonitoredItem == default)
                return null;

            var subscriptions = session!.Subscriptions
                .Where(x => x.MonitoredItems.Any(y => alarmMonitoredItem.NodeId.Equals(y.ResolvedNodeId.ToString())));

            return subscriptions.FirstOrDefault();
        }

        private Subscription CreateNewAlarmSubscription(AlarmSubscriptionMonitoredItem alarmMonitoredItem)
        {
            var keepAliveCount = Convert.ToUInt32(alarmMonitoredItem.HeartbeatInterval);
            var subscription = session!.Subscriptions.FirstOrDefault(x => alarmMonitoredItem.PublishingInterval.Equals(x.PublishingInterval));
            if (subscription == default)
            {
                subscription = new Subscription
                {
                    PublishingInterval = alarmMonitoredItem.PublishingInterval,
                    LifetimeCount = 100000,
                    KeepAliveCount = keepAliveCount > 0 ? keepAliveCount : 100000,
                    MaxNotificationsPerPublish = 1000,
                    Priority = 0,
                    PublishingEnabled = false
                };

                session!.AddSubscription(subscription);
                subscription.Create();
            }
            var item = this.CreateAlarmMonitoredItem(alarmMonitoredItem);
            subscription.AddItem(item);
            return subscription;
        }

        private Subscription ModifyAlarmSubscription(Subscription opcUaSubscription, AlarmSubscriptionMonitoredItem alarmMonitoredItem)
        {
            var existingItems = opcUaSubscription
                .MonitoredItems
                .Where(x => alarmMonitoredItem.NodeId.Equals(x.ResolvedNodeId.ToString()))
                .ToList();

            //TODO: Implement logic to detect a change in subscription, e.g. filter

            opcUaSubscription.RemoveItems(existingItems);// Notification of intent
            opcUaSubscription.ApplyChanges(); // enforces intent is executed
            return this.CreateNewAlarmSubscription(alarmMonitoredItem); // now re-add the monitored item
        }

        private MonitoredItem CreateAlarmMonitoredItem(AlarmSubscriptionMonitoredItem alarmSubscriptionMonitoredItem)
        {
            try
            {
                var alarmTypeNodeIds = alarmSubscriptionMonitoredItem.GetAlarmTypesAsNodeIds();

                var filter = new AlarmFilterDefinition
                {
                    AreaId = alarmSubscriptionMonitoredItem.NodeId,
                    Severity = alarmSubscriptionMonitoredItem.Severity,
                    IgnoreSuppressedOrShelved = alarmSubscriptionMonitoredItem.IgnoreSuppressedOrShelved,
                    EventTypes = alarmTypeNodeIds
                };

                // generate select clauses for all fields of all alarm types
                filter.SelectClauses = this.ConstructSelectClauses(alarmTypeNodeIds);

                // filter clauses based on the list of fields that should be included (if available in request)
                if (alarmSubscriptionMonitoredItem.AlarmFields != null && alarmSubscriptionMonitoredItem.AlarmFields.Any())
                {
                    var filteredClauses = filter.SelectClauses.Where(clause => alarmSubscriptionMonitoredItem.AlarmFields.Any(field => field.Equals(clause.ToString())));
                    filter.SelectClauses = new SimpleAttributeOperandCollection(filteredClauses);
                }

                // create monitored item based on the current filter settings
                var monitoredItem = filter.CreateMonitoredItem();

                foreach (var processor in this.alarmMonitoredItemMessageProcessors)
                    monitoredItem.Notification += processor.ProcessMessage; //If processor throws error the application crashes

                logger.LogTrace("Alarm monitored item with NodeId: {nodeId}, Sampling Interval: [{samplingInterval}] has been created successfully",
                    alarmSubscriptionMonitoredItem.NodeId,
                    monitoredItem.SamplingInterval);

                return monitoredItem;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unable to create monitored item with NodeId: [{nodeId}] | Error: {error}", alarmSubscriptionMonitoredItem.NodeId, ex);
                //TODO: subscribe to all items we can but let the error buble up as well
                throw;
            }
        }

        private SimpleAttributeOperandCollection ConstructSelectClauses(
            params NodeId[] eventTypeIds)
        {
            // browse the type model in the server address space to find the fields available for the event type.
            var selectClauses = new SimpleAttributeOperandCollection();

            // must always request the NodeId for the condition instances.
            // this can be done by specifying an operand with an empty browse path.
            var operand = new SimpleAttributeOperand();

            operand.TypeDefinitionId = ObjectTypeIds.BaseEventType;
            operand.AttributeId = Attributes.NodeId;
            operand.BrowsePath = new QualifiedNameCollection();

            selectClauses.Add(operand);

            // add the fields for the selected EventTypes.
            if (eventTypeIds != null)
            {
                for (var ii = 0; ii < eventTypeIds.Length; ii++)
                {
                    CollectFields(eventTypeIds[ii], selectClauses);
                }
            }

            // use BaseEventType as the default if no EventTypes specified.
            else
            {
                CollectFields(ObjectTypeIds.BaseEventType, selectClauses);
            }

            return selectClauses;
        }

        private void CollectFields(NodeId eventTypeId, SimpleAttributeOperandCollection eventFields)
        {
            // get the supertypes.
            var supertypes = BrowseSuperTypes(eventTypeId, false);

            if (supertypes == null)
            {
                return;
            }

            // process the types starting from the top of the tree.
            var foundNodes = new Dictionary<NodeId, QualifiedNameCollection>();
            var parentPath = new QualifiedNameCollection();

            for (var ii = supertypes.Count - 1; ii >= 0; ii--)
            {
                CollectFields((NodeId)supertypes[ii].NodeId, parentPath, eventFields, foundNodes);
            }

            // collect the fields for the selected type.
            CollectFields(eventTypeId, parentPath, eventFields, foundNodes);
        }

        private void CollectFields(
            NodeId nodeId,
            QualifiedNameCollection parentPath,
            SimpleAttributeOperandCollection eventFields,
            Dictionary<NodeId, QualifiedNameCollection> foundNodes)
        {
            // find all of the children of the field.
            var nodeToBrowse = new BrowseDescription();

            nodeToBrowse.NodeId = nodeId;
            nodeToBrowse.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse.IncludeSubtypes = true;
            nodeToBrowse.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable);
            nodeToBrowse.ResultMask = (uint)BrowseResultMask.All;

            var children = Browse(nodeToBrowse, false);

            if (children == null)
            {
                return;
            }

            // process the children.
            for (var ii = 0; ii < children.Count; ii++)
            {
                var child = children[ii];

                if (child.NodeId.IsAbsolute)
                {
                    continue;
                }

                // construct browse path.
                var browsePath = new QualifiedNameCollection(parentPath);
                browsePath.Add(child.BrowseName);

                // check if the browse path is already in the list.
                if (!ContainsPath(eventFields, browsePath))
                {
                    var field = new SimpleAttributeOperand();

                    field.TypeDefinitionId = ObjectTypeIds.BaseEventType;
                    field.BrowsePath = browsePath;
                    field.AttributeId = child.NodeClass == NodeClass.Variable ? Attributes.Value : Attributes.NodeId;

                    eventFields.Add(field);
                }

                // recusively find all of the children.
                var targetId = (NodeId)child.NodeId;

                // need to guard against loops.
                if (!foundNodes.ContainsKey(targetId))
                {
                    foundNodes.Add(targetId, browsePath);
                    CollectFields((NodeId)child.NodeId, browsePath, eventFields, foundNodes);
                }
            }
        }

        private bool ContainsPath(SimpleAttributeOperandCollection selectClause, QualifiedNameCollection browsePath)
        {
            for (var ii = 0; ii < selectClause.Count; ii++)
            {
                var field = selectClause[ii];

                if (field.BrowsePath.Count != browsePath.Count)
                {
                    continue;
                }

                var match = true;

                for (var jj = 0; jj < field.BrowsePath.Count; jj++)
                {
                    if (field.BrowsePath[jj] != browsePath[jj])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }

        private ReferenceDescriptionCollection BrowseSuperTypes(NodeId typeId, bool throwOnError)
        {
            var supertypes = new ReferenceDescriptionCollection();

            try
            {
                // find all of the children of the field.
                var nodeToBrowse = new BrowseDescription();

                nodeToBrowse.NodeId = typeId;
                nodeToBrowse.BrowseDirection = BrowseDirection.Inverse;
                nodeToBrowse.ReferenceTypeId = ReferenceTypeIds.HasSubtype;
                nodeToBrowse.IncludeSubtypes = false; // more efficient to use IncludeSubtypes=False when possible.
                nodeToBrowse.NodeClassMask = 0; // the HasSubtype reference already restricts the targets to Types. 
                nodeToBrowse.ResultMask = (uint)BrowseResultMask.All;

                var references = Browse(nodeToBrowse, throwOnError);

                while (references != null && references.Count > 0)
                {
                    // should never be more than one supertype.
                    supertypes.Add(references[0]);

                    // only follow references within this server.
                    if (references[0].NodeId.IsAbsolute)
                    {
                        break;
                    }

                    // get the references for the next level up.
                    nodeToBrowse.NodeId = (NodeId)references[0].NodeId;
                    references = Browse(nodeToBrowse, throwOnError);
                }

                // return complete list.
                return supertypes;
            }
            catch (Exception exception)
            {
                if (throwOnError)
                {
                    throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
                }

                return null;
            }
        }

        private ReferenceDescriptionCollection Browse(BrowseDescription nodeToBrowse, bool throwOnError)
        {
            try
            {
                var references = new ReferenceDescriptionCollection();

                // construct browse request.
                var nodesToBrowse = new BrowseDescriptionCollection();
                nodesToBrowse.Add(nodeToBrowse);

                session.Browse(
                    null,
                    null,
                    0,
                    nodesToBrowse,
                    out var results,
                    out var diagnosticInfos);

                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

                do
                {
                    // check for error.
                    if (StatusCode.IsBad(results[0].StatusCode))
                    {
                        throw new ServiceResultException(results[0].StatusCode);
                    }

                    // process results.
                    for (var ii = 0; ii < results[0].References.Count; ii++)
                    {
                        references.Add(results[0].References[ii]);
                    }

                    // check if all references have been fetched.
                    if (results[0].References.Count == 0 || results[0].ContinuationPoint == null)
                    {
                        break;
                    }

                    // continue browse operation.
                    var continuationPoints = new ByteStringCollection();
                    continuationPoints.Add(results[0].ContinuationPoint);

                    session.BrowseNext(
                        null,
                        false,
                        continuationPoints,
                        out results,
                        out diagnosticInfos);

                    ClientBase.ValidateResponse(results, continuationPoints);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);
                }
                while (true);

                //return complete list.
                return references;
            }
            catch (Exception exception)
            {
                if (throwOnError)
                {
                    throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
                }

                return null;
            }
        }
        #endregion

        #region [Call]
        private static void ValidateResponseDiagnostics(IList request, IList response, DiagnosticInfoCollection diagnosticInfoCollection)
        {
            ClientBase.ValidateResponse(response, request);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollection, request);
        }
        #endregion

        #region [Browse]
        private async Task<ReferenceDescriptionCollection> BrowseAsync(Session session, BrowseDescription browseDescription, ILogger logger, CancellationToken? cancellationToken = null)
        {
            var browseDescriptionCollection = new BrowseDescriptionCollection { browseDescription };

            var stoppingToken = cancellationToken ?? CancellationToken.None;

            var browseResult = await session.BrowseAsync(default,
                                default,
                                opcUaConfiguration.BrowseRequestedMaxReferencesPerNode,
                                browseDescriptionCollection,
                                stoppingToken);

            ValidateResponseDiagnostics(browseDescriptionCollection, browseResult.Results, browseResult.DiagnosticInfos);

            var comparer = new ReferenceDescriptionEqualityComparer();
            var continuationPoint = browseResult.Results[0].ContinuationPoint;
            var references = browseResult.Results[0].References.Distinct(comparer).ToList();

            logger.LogTrace("Browsed NodeId: {nodeId} and found [{referencesCount}] references!", browseDescription.NodeId, references.Count);

            while (continuationPoint != null)
            {
                logger.LogTrace($"NodeId: {browseDescription.NodeId} has continuationPoint .....");
                var additionalReferences = (await BrowseNextAsync(session, continuationPoint, stoppingToken)).Distinct(comparer).ToList();

                if (additionalReferences.Any())
                    references.AddRange(additionalReferences);

                logger.LogTrace($"Browsed continuationPoint,  NodeId: {browseDescription.NodeId} and found [{additionalReferences.Count}] references!");
            }
            return new ReferenceDescriptionCollection(references);
        }

        private static async Task<ReferenceDescriptionCollection> BrowseNextAsync(SessionClient session, byte[] continuationPoint, CancellationToken cancellationToken)
        {
            var continuationPoints = new ByteStringCollection { continuationPoint };

            var browseNextResponse = await session.BrowseNextAsync(
                null,
                false,
                continuationPoints,
                cancellationToken);

            ClientBase.ValidateResponse(browseNextResponse.Results, continuationPoints);
            ClientBase.ValidateDiagnosticInfos(browseNextResponse.DiagnosticInfos, continuationPoints);

            continuationPoint = browseNextResponse.Results[0].ContinuationPoint;
            return browseNextResponse.Results[0].References;
        }
        #endregion

        #region [Read]
        private static string GetNodeFriendlyDataType(Session session, NodeId dataTypeNodeId, int valueRank)
        {
            var dataType = session.NodeCache.Find(dataTypeNodeId);
            var dataTypeDisplayName = dataType?.DisplayName?.Text.ToLower() ?? "Unknown";
            return valueRank >= ValueRanks.OneOrMoreDimensions ? $"{dataTypeDisplayName}[]" : dataTypeDisplayName;
        }
        #endregion

        #region [Write]
        private static ReadValueIdCollection GetDataTypeAndValueRankQuery(NodeId variableNodeId)
        {
            return new ReadValueIdCollection
            {
                new ReadValueId
                {
                    NodeId = variableNodeId,
                    AttributeId = Attributes.DataType
                },
                new ReadValueId
                {
                    NodeId = variableNodeId,
                    AttributeId = Attributes.ValueRank
                }
            };
        }

        private BuiltInType GetSuperTypeAsBuiltInType(NodeId dataTypeId)
        {
            var dataTypeNode = session!.NodeCache.Find(dataTypeId);
            var superTypeNode = (NodeId)(dataTypeNode as Node)?.GetSuperType(session.TypeTree);
            var builtInType = TypeInfo.GetBuiltInType(superTypeNode);

            if (builtInType == BuiltInType.Null)
                builtInType = GetSuperTypeAsBuiltInType(superTypeNode);

            return builtInType;
        }

        private BuiltInType GetBuiltInType(NodeId dataTypeId)
        {
            var builtInType = TypeInfo.GetBuiltInType(dataTypeId);

            if (builtInType == BuiltInType.Null)
            {
                builtInType = GetSuperTypeAsBuiltInType(dataTypeId);
            }

            return builtInType;
        }
        #endregion
        #endregion
    }
}
