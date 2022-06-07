// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OMP.Connector.Application;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues;
using OMP.Connector.Infrastructure.OpcUa.Extensions;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;
using OMP.Connector.Infrastructure.OpcUa.States;
using Opc.Ua;
using Opc.Ua.Client;
using BrowseRequest = OMP.Connector.Domain.Schema.Request.Control.BrowseRequest;
using BrowseResponse = OMP.Connector.Domain.Schema.Responses.Control.BrowseResponse;

namespace OMP.Connector.Infrastructure.OpcUa
{
    public class OpcSession : IOpcSession
    {
        private readonly OpcUaConfiguration _opcUaSettings;
        private readonly IOpcSessionReconnectHandlerFactory _opcSessionReconnectHandlerFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private bool _disposed;
        private SemaphoreSlim _opcSessionSemaphore;
        private CancellationTokenSource _sessionCancellationTokenSource;
        private IOpcSessionReconnectHandler _reconnectHandler;
        private Session _session;
        private IRegisteredNodeStateManager _registeredNodeStateManager;
        private IComplexTypeSystem _complexTypeSystem;
        private readonly EndpointConfiguration _endpointConfiguration;
        private readonly IMapper _mapper;
        private readonly IUserIdentityProvider _identityProvider;
        private const string JsonTypeKey = "$type";
        private bool _reconnectIsDisabled = false;

        public OpcSession(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            ILoggerFactory loggerFactory,
            ApplicationConfiguration applicationConfiguration,
            IMapper mapper,
            IUserIdentityProvider identityProvider
            )
        {
            _opcUaSettings = connectorConfiguration.Value.OpcUa;
            _opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<OpcSession>();
            _applicationConfiguration = applicationConfiguration;
            _endpointConfiguration = EndpointConfiguration.Create(applicationConfiguration);
            _mapper = mapper;
            _identityProvider = identityProvider;
        }

        public IEnumerable<Subscription> Subscriptions => _session.Subscriptions;

        public Session Session
        {
            get => _session;
            set => _session = value;
        }

        #region [Public Members]

        public async Task ConnectAsync(EndpointDescription endpointDescription)
        {
            var identity = _identityProvider.GetUserIdentity(endpointDescription);
            await ConnectAsync(endpointDescription, identity);
        }
        
        private async Task ConnectAsync(EndpointDescription endpointDescription, IUserIdentity identity)
        {
            try
            {
                _opcSessionSemaphore = new SemaphoreSlim(1);
                _sessionCancellationTokenSource = new CancellationTokenSource();

                var locked = await LockSessionAsync().ConfigureAwait(false);

                if (!locked) { return; }
                
                var sessionName = $"{_applicationConfiguration.ApplicationUri}:{Guid.NewGuid()}"; 
                var endPointConfiguration = EndpointConfiguration.Create(_applicationConfiguration);
                var configuredEndpoint = new ConfiguredEndpoint(endpointDescription.Server, endPointConfiguration);
                configuredEndpoint.Update(endpointDescription);
                
                _session = await Session.Create(
                    _applicationConfiguration,
                    configuredEndpoint,
                    true,
                    sessionName,
                    100000,
                    identity,
                    default);

                _session.KeepAliveInterval = _opcUaSettings.KeepAliveIntervalInSeconds.ToMilliseconds();
                _session.KeepAlive += SessionOnKeepAlive;
                _session.OperationTimeout = _opcUaSettings.OperationTimeoutInSeconds.ToMilliseconds();

                await LoadComplexTypeSystemAsync();

                _registeredNodeStateManager ??= _opcUaSettings.EnableRegisteredNodes
                    ? new RegisteredNodeStateManager(_session, _loggerFactory.CreateLogger<RegisteredNodeStateManager>(), _opcUaSettings.RegisterNodeBatchSize)
                    : null;
            }
            finally
            {
                ReleaseSession();
            }
        }

        public async Task ConnectAsync(string opcUaServerUrl)
        {
            var endpointDescriptionCollection = GetEndpoints(opcUaServerUrl);
            var endpointDescriptions = endpointDescriptionCollection.OrderByDescending(e => e.SecurityLevel);
            foreach (var endpointDescription in endpointDescriptions)
            {
                try
                {
                    await ConnectAsync(endpointDescription);

                    var message =
                        $"Session created to Endpoint with: [{endpointDescription.EndpointUrl}] with SecurityMode: [{endpointDescription.SecurityMode}] and Level: [{endpointDescription.SecurityLevel}]";
                    _logger.Information(message);
                    break;
                }
                catch (Exception e)
                {
                    var message =
                        $"Unable to create Session to Endpoint with: [{endpointDescription.EndpointUrl}] with SecurityMode: [{endpointDescription.SecurityMode}] and Level: [{endpointDescription.SecurityLevel}] = {e.Message}::{e.InnerException?.Message}";
                    _logger.Warning(message);
                    await Task.Delay(500); // fix for Milo server not being happy with endpoint connections in quick succession
                }
            }

            if (_session == default)
                throw new Exception($"Unable to create a session to OPC Server: [{endpointDescriptionCollection.FirstOrDefault()?.EndpointUrl}] on all its endpoints");
        }

        public async Task UseAsync(Action<Session, IComplexTypeSystem> action)
        {
            var locked = await LockSessionAsync().ConfigureAwait(false);

            if (!locked)
                throw new Exception("Could not obtain session lock. Action could not be executed.");

            try
            {
                if (_session.Disposed || !_session.Connected)
                    throw new Exception("Not Connected");

                action.Invoke(_session, _complexTypeSystem);
            }
            finally
            {
                ReleaseSession();
            }
        }

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds)
            => _registeredNodeStateManager.GetRegisteredNodeIds(nodeIds);

        public TNode GetNode<TNode>(NodeId nodeId) where TNode : class
        {
            return _session.NodeCache.Find(nodeId) as TNode;
        }

        public async Task<Type> LoadTypeAsync(NodeId nodeId)
        {
            if (nodeId == null)
                return null;

            return await _complexTypeSystem.LoadType(nodeId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region UseAsync Replacements
        public Task<IEnumerable<BrowseResponse>> BrowseNodesAsync(
            IEnumerable<(NodeId NodeId, BrowseRequest Command)> nodeIdCommands,
            Func<BrowseRequest, BrowseResponse> constructResultFunc)
        {
            var browsedResults = new List<BrowseResponse>();

            foreach (var (nodeId, command) in nodeIdCommands)
            {
                var browseResult = constructResultFunc.Invoke(command);
                try
                {
                    var node = _session.ReadNode(nodeId);
                    browseResult.Node = _mapper.Map<BrowsedOpcNode>(node);
                    browseResult.Message = ServiceResult.Create(StatusCodes.Good, default, default).ToString();
                    browseResult.OpcUaCommandType = OpcUaCommandType.Browse;

                    _logger.Debug(
                        $"Executed {command.GetType().Name} on NodeId: [{command.NodeId}]" +
                        $" and Endpoint: [{_session.Endpoint.EndpointUrl}] " +
                        $"with Node: {JsonConvert.SerializeObject(browseResult.Node)}");
                }
                catch (Exception e)
                {
                    var message = e.GetMessage();
                    browseResult.Message = message;
                    _logger.Error(message);
                }

                browsedResults.Add(browseResult);
            }
            return Task.FromResult(browsedResults.AsEnumerable());
        }

        public void ReadNodes(List<NodeId> nodeIds, int batchSize, List<object> values, List<ServiceResult> errors)
        {
            var omitExpectedTypes = nodeIds.Select(_ => (Type)null).ToList();
            var batchHandler = new BatchHandler<NodeId>(batchSize, ReadBatch(_session, values, errors, omitExpectedTypes));
            batchHandler.RunBatches(nodeIds.ToList());

            _logger.Debug($"Executed Read commands. Endpoint: [{_session.Endpoint.EndpointUrl}]");
        }

        public StatusCodeCollection WriteNodes(WriteValueCollection writeValues)
        {
            _session.Write(default, writeValues, out var statusCodeCollection, out _);
            return statusCodeCollection;
        }

        public void ConvertToOpcUaTypedValues(IEnumerable<WriteRequestWrapper> writeRequests)
        {
            var writeCommandsArray = writeRequests as WriteRequestWrapper[] ?? writeRequests.ToArray();
            var dataTypeIds = GetCommandReadValueIdsByAttribute(writeCommandsArray, Attributes.DataType);
            var valueRankIds = GetCommandReadValueIdsByAttribute(writeCommandsArray, Attributes.ValueRank);
            var dataTypesCollection = ReadDataValueCollection(_session, dataTypeIds);
            var valueRanksCollection = ReadDataValueCollection(_session, valueRankIds);
            var commandsTuple = writeCommandsArray.Zip(dataTypesCollection).Zip(valueRanksCollection);

            foreach (var ((command, dataType), rank) in commandsTuple)
            {
                if (!IsValidNode(command.Value, dataType, rank)) continue;

                try
                {
                    var valueRank = int.Parse(rank.Value.ToString()!);
                    switch (valueRank)
                    {
                        case ValueRanks.Scalar:
                            ParseCommandScalarValueAsync(command, dataType, _session).GetAwaiter().GetResult();
                            break;
                        case ValueRanks.OneDimension:
                            ParseCommandArrayValueAsync(command, dataType).GetAwaiter().GetResult();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error occurred while parsing the write command value for node: {command.NodeId}");
                }
            }
        }
        #endregion

        #endregion

        #region [Protected Members]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing) return;

            _logger.Trace($"Closing and disposing session: {_session?.Endpoint?.EndpointUrl}");

            _sessionCancellationTokenSource?.Cancel();
            _sessionCancellationTokenSource?.Dispose();

            _reconnectHandler?.Dispose();

            _opcSessionSemaphore?.Dispose();
            _opcSessionSemaphore = null;

            _registeredNodeStateManager?.Dispose();
            _registeredNodeStateManager = null;

            _complexTypeSystem = null;

            if (_session == null) return;

            _session.KeepAlive -= SessionOnKeepAlive;
            _session.RemoveSubscriptions(_session.Subscriptions.ToList());
            _session.Close();
            _session.Dispose();

            _disposed = true;
        }
        #endregion

        #region [Private Members]

        private async Task LoadComplexTypeSystemAsync()
        {
            if (_complexTypeSystem == null)
            {
                _logger.Trace("Loading OPC UA complex type system...");
                var complexTypeSystemWrapper = new ComplexTypeSystemWrapper(_session);
                _complexTypeSystem = complexTypeSystemWrapper;
                await complexTypeSystemWrapper.Load();
                _logger.Trace("Finished loading OPC UA complex type system.");
            }
        }

        private EndpointDescriptionCollection GetEndpoints(string endpointAddress)
        {
            var uri = new Uri(endpointAddress);
            using var discoveryClient = DiscoveryClient.Create(uri, _endpointConfiguration);
            var endpoints = discoveryClient.GetEndpoints(default);

            _logger.Debug($"Discovered [{endpoints.Count}] endpoints for Server: [{endpointAddress}]");
            return endpoints;
        }

        private void SessionOnKeepAlive(Session session, KeepAliveEventArgs e)
        {
            try
            {
                if (_reconnectIsDisabled) return;

                if (session.SessionName != _session.SessionName)
                    return;

                if (!ReferenceEquals(session, _session))
                    _session = session;

                // start reconnect sequence on communication error.
                if (e != null && !ServiceResult.IsBad(e.Status))
                    return;

                var communicationError = $"Communication error: [{e?.Status}] on Endpoint: [{session.Endpoint?.EndpointUrl}]";

                if (_opcUaSettings.ReconnectIntervalInSeconds <= 0)
                {
                    _logger.Warning(communicationError);
                    _logger.Warning("Reconnect is disabled. To enable reconnect, set the ReconnectIntervalInSeconds value to greater than 0");
                    _reconnectIsDisabled = true;
                    return;
                }

                if (IsReconnectHandlerHealthy())
                    return;

                var lockObtained = LockSessionAsync().GetAwaiter().GetResult();
                if (!lockObtained) { return; }

                _logger.Warning(communicationError);

                DisposeUnHealthyReconnectHandler();

                _reconnectHandler = _opcSessionReconnectHandlerFactory.Create();
                _reconnectHandler.BeginReconnect(this, _session, _opcUaSettings.ReconnectIntervalInSeconds.ToMilliseconds(), _registeredNodeStateManager, ServerReconnectComplete);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool IsReconnectHandlerHealthy()
            => _reconnectHandler is { IsHealthy: true };

        private void DisposeUnHealthyReconnectHandler()
        {
            if (_reconnectHandler is null)
                return;

            if (!_reconnectHandler.IsHealthy)
                _reconnectHandler.Dispose();
        }

        private void ServerReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                _logger.Information($"Server successfully reconnected: {_session?.Endpoint?.EndpointUrl}");

                if (!ReferenceEquals(sender, _reconnectHandler))
                    return;

                _reconnectHandler?.Dispose();
                _reconnectHandler = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                ReleaseSession();
            }
        }

        private async Task<bool> LockSessionAsync()
        {
            _logger.Trace($"Locking session on Endpoint: [{_session?.Endpoint?.EndpointUrl}]");
            bool result;
            try
            {
                result = await _opcSessionSemaphore.WaitAsync(TimeSpan.FromSeconds(_opcUaSettings.AwaitSessionLockTimeoutSeconds), _sessionCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void ReleaseSession()
        {
            _opcSessionSemaphore?.Release();
            _logger.Trace($"Releasing session on Endpoint: [{_session?.Endpoint?.EndpointUrl}]");
        }

        private Action<NodeId[]> ReadBatch(Session session, List<object> values, List<ServiceResult> errors, List<Type> omitExpectedTypes)
        {
            return (items) =>
            {
                _logger.Trace($"Reading {items.Length} nodes...");
                session.ReadValues(
                    items,
                    omitExpectedTypes.Take(items.Length).ToList(),
                    out var batchValues,
                    out var batchErrors);

                values.AddRange(batchValues);
                errors.AddRange(batchErrors);
            };
        }

        private static bool IsValidNode(object value, DataValue dataTypeNode, DataValue rank)
            => dataTypeNode.StatusCode == StatusCodes.Good && value != default && !rank.Equals(default);

        private static List<ReadValueId> GetCommandReadValueIdsByAttribute(WriteRequestWrapper[] writeCommands, uint attributeId)
        {
            var readValues = from command in writeCommands
                             select new ReadValueId
                             {
                                 NodeId = command.NodeId,
                                 AttributeId = attributeId
                             };
            return readValues.ToList();
        }

        private DataValueCollection ReadDataValueCollection(Session session, List<ReadValueId> valueIds)
        {
            session.Read(default, 0, TimestampsToReturn.Neither, new ReadValueIdCollection(valueIds),
                out var dataValueCollection, out _);
            return dataValueCollection;
        }

        private async Task ParseCommandArrayValueAsync(WriteRequestWrapper command, DataValue dataValue)
        {
            var (dataTypeId, builtInType) = GetBuiltInType(dataValue.Value);

            if (IsArrayOfPrimitiveType(command.Value, out var primitiveArray))
            {
                var array = primitiveArray.ToArray();
                if (array.GetType() == typeof(decimal[])) //decimal is used to accommodate special edge case for UInt64 values
                {
                    var systemType = TypeInfo.GetSystemType(builtInType, -1);
                    command.Value = CastDecimalArrayToIntegerArray((decimal[])array, systemType);
                }
                else
                    command.Value = TypeInfo.Cast(array, builtInType);
            }
            else
            {
                command.Value = await CreateOpcUaStructArrayAsync(command, (NodeId)dataValue.Value);
            }
        }

        private object CastDecimalArrayToIntegerArray(decimal[] array, Type targetType)
        {
            return targetType switch
            {
                { } when targetType == typeof(byte) => CastArray(array, Convert.ToByte),
                { } when targetType == typeof(sbyte) => CastArray(array, Convert.ToSByte),
                { } when targetType == typeof(short) => CastArray(array, Convert.ToInt16),
                { } when targetType == typeof(int) => CastArray(array, Convert.ToInt32),
                { } when targetType == typeof(long) => CastArray(array, Convert.ToInt64),
                { } when targetType == typeof(ushort) => CastArray(array, Convert.ToUInt16),
                { } when targetType == typeof(uint) => CastArray(array, Convert.ToUInt32),
                { } when targetType == typeof(ulong) => CastArray(array, Convert.ToUInt64),
                _ => array
            };
        }

        private object CastArray<T>(decimal[] array, Converter<decimal, T> converter)
        {
            return Array.ConvertAll(array, converter);
        }

        private static bool IsArrayOfPrimitiveType(object value, out IPrimitiveArray primitiveArray)
        {
            if (typeof(IPrimitiveArray).IsAssignableFrom(value.GetType()))
            {
                primitiveArray = value as IPrimitiveArray;
                return true;
            }

            primitiveArray = null;
            return false;
        }

        private async Task ParseCommandScalarValueAsync(WriteRequestWrapper command, DataValue dataValue, Session session)
        {
            var (dataTypeId, builtInType) = GetBuiltInType(dataValue.Value);
            if (builtInType == BuiltInType.Null)
            {
                builtInType = GetSuperTypeAsBuiltInType(session, dataTypeId);
            }

            command.Value = builtInType == BuiltInType.ExtensionObject
                ? await this.CreateOpcUaStructAsync(command, dataTypeId, session).ConfigureAwait(false)
                : command.Value is WriteRequestStringValue wrsv
                    ? builtInType == BuiltInType.DateTime
                        ? XmlConvert.ToDateTime(wrsv.ToString(), XmlDateTimeSerializationMode.RoundtripKind)
                        : TypeInfo.Cast(wrsv.ToString(), builtInType)
                    : TypeInfo.Cast(command.Value, builtInType);
        }

        private static BuiltInType GetSuperTypeAsBuiltInType(Session session, NodeId dataTypeId)
        {
            var dataTypeNode = session.NodeCache.Find(dataTypeId);
            var superTypeNode = (NodeId)(dataTypeNode as Node)?.GetSuperType(session.TypeTree);
            var builtInType = TypeInfo.GetBuiltInType(superTypeNode);

            if (builtInType == BuiltInType.Null)
                builtInType = GetSuperTypeAsBuiltInType(session, superTypeNode);

            return builtInType;
        }

        private (NodeId, BuiltInType) GetBuiltInType(object value)
        {
            var dataTypeId = (NodeId)value;
            return (dataTypeId, TypeInfo.GetBuiltInType(dataTypeId));
        }

        private async Task<object> CreateOpcUaStructAsync(WriteRequestWrapper command, NodeId dataTypeId, Session session)
        {
            if (command.Value is WriteRequestValues values)
            {
                var structType = await LoadStructTypeAsync(dataTypeId).ConfigureAwait(false);
                var dictionary = values.ToDictionary(x => x.Key, y => y.Value as object);
                return BuildOpcUaStruct(structType, dictionary);
            }
            else
            {
                var dataTypeNode = session.NodeCache.Find(dataTypeId);
                throw new Exception($"Data type is not a valid struct: {dataTypeNode.DisplayName}");
            }
        }

        private async Task<Array> CreateOpcUaStructArrayAsync(WriteRequestWrapper command, NodeId dataTypeId)
        {
            var structType = await LoadStructTypeAsync(dataTypeId).ConfigureAwait(false);
            return BuildOpcUaStructArray(structType, UnboxStructArray(command.Value));
        }

        private async Task<Type> LoadStructTypeAsync(NodeId dataTypeId)
        {
            return await LoadTypeAsync(dataTypeId);
        }

        private List<IDictionary<string, object>> UnboxStructArray(object listObject)
        {

            if (listObject is WriteRequestValues writeRequestValues)
            {
                var list = new List<IDictionary<string, object>>();
                foreach (var item in writeRequestValues)
                {
                    list.Add((item.Value as WriteRequestValues)?.ToDictionary(x => x.Key, y => y.Value as object));
                }
                return list;
            }

            if (listObject is List<IDictionary<string, object>> dictList)
                return dictList;

            throw new Exception($"Struct collection type not supported: {listObject.GetType().FullName}");
        }

        private object BuildOpcUaStruct(Type structType, IDictionary<string, object> structElements)
        {
            var structInstance = Activator.CreateInstance(structType);
            foreach (var kvp in structElements)
            {
                if (kvp.Key.Equals(JsonTypeKey)) continue;
                var propInfo = structType.GetProperty(kvp.Key);
                var value = kvp.Value;

                if (propInfo is { } && value.GetType() != propInfo.PropertyType)
                {
                    if (typeof(Array).IsAssignableFrom(propInfo.PropertyType))
                    {
                        value = BuildOpcUaStructArray(propInfo.PropertyType.GetElementType(), UnboxStructArray(value));
                    }
                    else if (propInfo.PropertyType.FullName != null && propInfo.PropertyType.FullName.StartsWith(Constants.NativeOpcUaNameSpace))
                    {
                        value = propInfo.PropertyType switch
                        {
                            { } when propInfo.PropertyType == typeof(StringCollection) => CastArray(value, propInfo.PropertyType, x => x.ToString()),
                            { } when propInfo.PropertyType == typeof(BooleanCollection) => CastArray(value, propInfo.PropertyType, Convert.ToBoolean),
                            { } when propInfo.PropertyType == typeof(ByteCollection) => CastArray(value, propInfo.PropertyType, Convert.ToByte),
                            { } when propInfo.PropertyType == typeof(ByteStringCollection) => CastArray(value, propInfo.PropertyType, ConvertToByteArray),
                            { } when propInfo.PropertyType == typeof(SByteCollection) => CastArray(value, propInfo.PropertyType, Convert.ToSByte),
                            { } when propInfo.PropertyType == typeof(DateTimeCollection) => CastArray(value, propInfo.PropertyType, Convert.ToDateTime),
                            { } when propInfo.PropertyType == typeof(DoubleCollection) => CastArray(value, propInfo.PropertyType, Convert.ToDouble),
                            { } when propInfo.PropertyType == typeof(FloatCollection) => CastArray(value, propInfo.PropertyType, Convert.ToSingle),
                            { } when propInfo.PropertyType == typeof(Int16Collection) => CastArray(value, propInfo.PropertyType, Convert.ToInt16),
                            { } when propInfo.PropertyType == typeof(Int32Collection) => CastArray(value, propInfo.PropertyType, Convert.ToInt32),
                            { } when propInfo.PropertyType == typeof(Int64Collection) => CastArray(value, propInfo.PropertyType, Convert.ToInt64),
                            { } when propInfo.PropertyType == typeof(UInt16Collection) => CastArray(value, propInfo.PropertyType, Convert.ToUInt16),
                            { } when propInfo.PropertyType == typeof(UInt32Collection) => CastArray(value, propInfo.PropertyType, Convert.ToUInt32),
                            { } when propInfo.PropertyType == typeof(UInt64Collection) => CastArray(value, propInfo.PropertyType, Convert.ToUInt64),
                            { } when propInfo.PropertyType == typeof(UuidCollection) => CastArray(value, propInfo.PropertyType, x => Guid.Parse(x.ToString()!)),
                            _ => value
                        };
                    }
                    else
                    {
                        if (value is WriteRequestStringValue wrsv) { value = wrsv.ToString(); }
                        var converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                        value = converter.CanConvertFrom(value.GetType())
                            ? converter.ConvertFrom(value)
                            : Convert.ChangeType(value, propInfo.PropertyType);
                    }
                }
                propInfo?.SetValue(structInstance, value);
            }
            return structInstance;
        }

        private Array BuildOpcUaStructArray(Type structType, List<IDictionary<string, object>> structs)
        {
            var structArray = Array.CreateInstance(structType, structs.Count);
            for (var i = 0; i < structs.Count; i++)
            {
                structArray.SetValue(BuildOpcUaStruct(structType, structs[i]), i);
            }
            return structArray;
        }

        private static byte[] ConvertToByteArray(object item)
        {
            var converter = TypeDescriptor.GetConverter(item.GetType());
            return (byte[])converter.ConvertTo(item, typeof(byte[]));
        }

        private static object CastArray<T>(object value, Type propertyType, Converter<object, T> converter)
            => Activator.CreateInstance(propertyType, Array.ConvertAll((value as object[])!, converter));
        #endregion
    }
}
