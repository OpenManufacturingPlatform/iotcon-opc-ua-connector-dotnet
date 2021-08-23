using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using OMP.Connector.Infrastructure.Kafka.ComplexTypes;
using OMP.Connector.Infrastructure.Kafka.Reconnect;
using OMP.Connector.Infrastructure.Kafka.States;
using Opc.Ua;
using Opc.Ua.Client;
using BrowseRequest = OMP.Connector.Domain.Schema.Request.Control.BrowseRequest;
using BrowseResponse = OMP.Connector.Domain.Schema.Responses.Control.BrowseResponse;

[assembly: InternalsVisibleTo("IoTPE.Connector.Application.Tests")]

namespace OMP.Connector.Infrastructure.Kafka
{
    public class OpcSession : IOpcSession
    {
        private readonly OpcUaConfiguration _opcUaSettings;
        private readonly IOpcSessionReconnectHandlerFactory _opcSessionReconnectHandlerFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly int _reconnectInterval;
        private bool _disposed;
        private SemaphoreSlim _opcSessionSemaphore;
        private CancellationTokenSource _sessionCancellationTokenSource;
        private IOpcSessionReconnectHandler _reconnectHandler;
        private Guid _sessionName;
        private Session _session;
        private IRegisteredNodeStateManager _registeredNodeStateManager;
        private ComplexTypeSystem _complexTypeSystem;
        private readonly EndpointConfiguration _endpointConfiguration;
        private readonly IMapper _mapper;
        private const string JsonTypeKey = "$type";


        public OpcSession(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcSessionReconnectHandlerFactory opcSessionReconnectHandlerFactory,
            ILoggerFactory loggerFactory,
            ApplicationConfiguration applicationConfiguration,
            IMapper mapper
            )
        {
            this._opcUaSettings = connectorConfiguration.Value.OpcUa;
            this._opcSessionReconnectHandlerFactory = opcSessionReconnectHandlerFactory;
            this._loggerFactory = loggerFactory;
            this._logger = this._loggerFactory.CreateLogger<OpcSession>();
            this._applicationConfiguration = applicationConfiguration;
            this._endpointConfiguration = EndpointConfiguration.Create(applicationConfiguration);
            this._mapper = mapper;
        }

        public IEnumerable<Subscription> Subscriptions => this._session.Subscriptions;

        internal Session Session
        {
            get => this._session;
            set => this._session = value;
        }

        #region [Public Members]
        public async Task ConnectAsync(EndpointDescription endpointDescription)
        {
            try
            {
                this._opcSessionSemaphore = new SemaphoreSlim(1);
                this._sessionCancellationTokenSource = new CancellationTokenSource();

                var locked = await this.LockSessionAsync().ConfigureAwait(false);

                if (!locked) { return; }

                this._sessionName = Guid.NewGuid();
                var endPointConfiguration = EndpointConfiguration.Create(this._applicationConfiguration);
                var configuredEndpoint = new ConfiguredEndpoint(endpointDescription.Server, endPointConfiguration);
                configuredEndpoint.Update(endpointDescription);

                this._session = await Session.Create(
                    this._applicationConfiguration,
                    configuredEndpoint,
                    true,
                    this._sessionName.ToString(),
                    100000,
                    default,
                    default);

                this._session.KeepAlive += this.SessionOnKeepAlive;
                this._session.OperationTimeout = (int)TimeSpan.FromSeconds(this._opcUaSettings.OperationTimeoutInSeconds).TotalMilliseconds;

                await this.LoadComplexTypeSystemAsync();

                this._registeredNodeStateManager ??= this._opcUaSettings.EnableRegisteredNodes
                    ? new RegisteredNodeStateManager(this._session, this._loggerFactory.CreateLogger<RegisteredNodeStateManager>(), this._opcUaSettings.RegisterNodeBatchSize)
                    : null;
            }
            finally
            {
                this.ReleaseSession();
            }
        }

        public async Task ConnectAsync(string opcUaServerUrl)
        {
            var endpointDescriptionCollection = this.GetEndpoints(opcUaServerUrl);
            var endpointDescriptions = endpointDescriptionCollection.OrderByDescending(e => e.SecurityLevel);
            foreach (var endpointDescription in endpointDescriptions)
            {
                try
                {
                    await this.ConnectAsync(endpointDescription);

                    var message =
                        $"Session created to Endpoint with: [{endpointDescription.EndpointUrl}] with SecurityMode: [{endpointDescription.SecurityMode}] and Level: [{endpointDescription.SecurityLevel}]";
                    this._logger.Information(message);
                    break;
                }
                catch (Exception e)
                {
                    var message =
                        $"Unable to create Session to Endpoint with: [{endpointDescription.EndpointUrl}] with SecurityMode: [{endpointDescription.SecurityMode}] and Level: [{endpointDescription.SecurityLevel}] = {e.Message}::{e.InnerException?.Message}";
                    this._logger.Warning(message);
                }
            }

            if (this._session == default)
                throw new Exception($"Unable to create a session to OPC Server: [{endpointDescriptionCollection.FirstOrDefault()?.EndpointUrl}] on all its endpoints");
        }

        public async Task UseAsync(Action<Session, IComplexTypeSystem> action)
        {
            var locked = await this.LockSessionAsync().ConfigureAwait(false);

            if (!locked)
                throw new Exception("Could not obtain session lock. Action could not be executed.");

            try
            {
                if (this._session.Disposed || !this._session.Connected)
                    throw new Exception("Not Connected");

                action.Invoke(this._session, this._complexTypeSystem);
            }
            finally
            {
                this.ReleaseSession();
            }
        }

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds)
            => this._registeredNodeStateManager.GetRegisteredNodeIds(nodeIds);

        public TNode GetNode<TNode>(NodeId nodeId) where TNode : class
        {
            return this._session.NodeCache.Find(nodeId) as TNode;
        }

        public async Task<Type> LoadTypeAsync(NodeId nodeId)
        {
            if (nodeId == null)
                return null;

            return await this._complexTypeSystem.LoadType(nodeId);
        }

        public void Dispose()
        {
            this.Dispose(true);
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
                    var node = this._session.ReadNode(nodeId);
                    browseResult.Node = this._mapper.Map<BrowsedOpcNode>(node);
                    browseResult.Message = ServiceResult.Create(StatusCodes.Good, default, default).ToString();
                    browseResult.OpcUaCommandType = OpcUaCommandType.Browse;

                    this._logger.Debug(
                        $"Executed {command.GetType().Name} on NodeId: [{command.NodeId}]" +
                        $" and Endpoint: [{this._session.Endpoint.EndpointUrl}] " +
                        $"with Node: {JsonConvert.SerializeObject(browseResult.Node)}");
                }
                catch (Exception e)
                {
                    var message = e.GetMessage();
                    browseResult.Message = message;
                    this._logger.Error(message);
                }

                browsedResults.Add(browseResult);
            }
            return Task.FromResult(browsedResults.AsEnumerable());
        }

        public void ReadNodes(List<NodeId> nodeIds, int batchSize, List<object> values, List<ServiceResult> errors)
        {
            var omitExpectedTypes = nodeIds.Select(_ => (Type)null).ToList();
            var batchHandler = new BatchHandler<NodeId>(batchSize, this.ReadBatch(this._session, values, errors, omitExpectedTypes));
            batchHandler.RunBatches(nodeIds.ToList());

            this._logger.Debug($"Executed Read commands. Endpoint: [{this._session.Endpoint.EndpointUrl}]");
        }

        public StatusCodeCollection WriteNodes(WriteValueCollection writeValues)
        {
            this._session.Write(default, writeValues, out var statusCodeCollection, out _);
            return statusCodeCollection;
        }

        public void ConvertToOpcUaTypedValues(IEnumerable<WriteRequestWrapper> writeRequests)
        {
            var writeCommandsArray = writeRequests as WriteRequestWrapper[] ?? writeRequests.ToArray();
            var dataTypeIds = GetCommandReadValueIdsByAttribute(writeCommandsArray, Attributes.DataType);
            var valueRankIds = GetCommandReadValueIdsByAttribute(writeCommandsArray, Attributes.ValueRank);
            var dataTypesCollection = this.ReadDataValueCollection(this._session, dataTypeIds);
            var valueRanksCollection = this.ReadDataValueCollection(this._session, valueRankIds);
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
                            this.ParseCommandScalarValueAsync(command, dataType, this._session).GetAwaiter().GetResult();
                            break;
                        case ValueRanks.OneDimension:
                            this.ParseCommandArrayValueAsync(command, dataType).GetAwaiter().GetResult();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this._logger.Error(ex, $"Error occurred while parsing the write command value for node: {command.NodeId}");
                }
            }
        }
        #endregion

        #endregion

        #region [Protected Members]
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed || !disposing) return;

            this._logger.Trace($"Closing and disposing session: {this._session?.Endpoint?.EndpointUrl}");

            this._sessionCancellationTokenSource?.Cancel();
            this._sessionCancellationTokenSource?.Dispose();

            this._reconnectHandler?.Dispose();

            this._opcSessionSemaphore?.Dispose();
            this._opcSessionSemaphore = null;

            this._registeredNodeStateManager?.Dispose();
            this._registeredNodeStateManager = null;

            this._complexTypeSystem = null;

            if (this._session == null) return;

            this._session.KeepAlive -= this.SessionOnKeepAlive;
            this._session.RemoveSubscriptions(this._session.Subscriptions.ToList());
            this._session.Close();
            this._session.Dispose();

            this._disposed = true;
        }
        #endregion

        #region [Private Members]

        private async Task LoadComplexTypeSystemAsync()
        {
            if (this._complexTypeSystem == null)
            {
                this._logger.Trace("Loading OPC UA complex type system...");
                this._complexTypeSystem = new ComplexTypeSystem(this._session);
                await this._complexTypeSystem.Load();
                this._logger.Trace("Finished loading OPC UA complex type system.");
            }
        }

        private EndpointDescriptionCollection GetEndpoints(string endpointAddress)
        {
            var uri = new Uri(endpointAddress);
            using var discoveryClient = DiscoveryClient.Create(uri, this._endpointConfiguration);
            var endpoints = discoveryClient.GetEndpoints(default);

            this._logger.Debug($"Discovered [{endpoints.Count}] endpoints for Server: [{endpointAddress}]");
            return endpoints;
        }

        private void SessionOnKeepAlive(Session session, KeepAliveEventArgs e)
        {
            try
            {
                if (session.SessionName != this._session.SessionName)
                    return;

                if (!ReferenceEquals(session, this._session))
                    this._session = session;

                // start reconnect sequence on communication error.
                if (e != null && !ServiceResult.IsBad(e.Status))
                    return;

                this._logger.Warning($"Communication error: [{e?.Status}] on Endpoint: [{session.Endpoint?.EndpointUrl}]");

                if (this._reconnectInterval <= 0)
                    return;

                if (this.IsReconnectHandlerHealthy())
                    return;

                var locked = this.LockSessionAsync().GetAwaiter().GetResult();
                if (!locked) { return; }

                this.DisposeUnHealthyReconnectHandler();

                this._reconnectHandler = this._opcSessionReconnectHandlerFactory.Create();
                this._reconnectHandler.BeginReconnect(this, this._session, this._reconnectInterval, this._registeredNodeStateManager, this.ServerReconnectComplete);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex);
            }
        }

        private bool IsReconnectHandlerHealthy()
            => this._reconnectHandler is { IsHealthy: true };

        private void DisposeUnHealthyReconnectHandler()
        {
            if (this._reconnectHandler is null)
                return;

            if (!this._reconnectHandler.IsHealthy)
                this._reconnectHandler.Dispose();
        }

        private void ServerReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                this._logger.Information($"Server successfully reconnected: {this._session?.Endpoint?.EndpointUrl}");

                if (!ReferenceEquals(sender, this._reconnectHandler))
                    return;

                this._reconnectHandler?.Dispose();
                this._reconnectHandler = null;
            }
            catch (Exception ex)
            {
                this._logger.Error(ex);
            }
            finally
            {
                this.ReleaseSession();
            }
        }

        private async Task<bool> LockSessionAsync()
        {
            this._logger.Trace($"Locking session on Endpoint: [{this._session?.Endpoint?.EndpointUrl}]");
            bool result;
            try
            {
                result = await this._opcSessionSemaphore.WaitAsync(TimeSpan.FromSeconds(this._opcUaSettings.AwaitSessionLockTimeoutSeconds), this._sessionCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void ReleaseSession()
        {
            this._opcSessionSemaphore?.Release();
            this._logger.Trace($"Releasing session on Endpoint: [{this._session?.Endpoint?.EndpointUrl}]");
        }

        private Action<NodeId[]> ReadBatch(Session session, List<object> values, List<ServiceResult> errors, List<Type> omitExpectedTypes)
        {
            return (items) =>
            {
                this._logger.Trace($"Reading {items.Length} nodes...");
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
            var (dataTypeId, builtInType) = this.GetBuiltInType(dataValue.Value);

            if (IsArrayOfPrimitiveType(command.Value, out var primitiveArray))
            {
                var array = primitiveArray.ToArray();
                if (array.GetType() == typeof(decimal[])) //decimal is used to accommodate special edge case for UInt64 values
                {
                    var systemType = TypeInfo.GetSystemType(builtInType, -1);
                    command.Value = this.CastDecimalArrayToIntegerArray((decimal[]) array, systemType);
                }
                else
                    command.Value = TypeInfo.Cast(array, builtInType);
            }
            else
            {
                await this.CreateOpcUaStructArrayAsync(command, (NodeId)dataValue.Value);
            }
        }

        private object CastDecimalArrayToIntegerArray(decimal[] array, Type targetType)
        {
            return targetType switch
            {
                { } when targetType == typeof(Byte) => this.CastArray(array, Convert.ToByte),
                { } when targetType == typeof(SByte) => this.CastArray(array, Convert.ToSByte),
                { } when targetType == typeof(Int16) => this.CastArray(array, Convert.ToInt16),
                { } when targetType == typeof(Int32) => this.CastArray(array, Convert.ToInt32),
                { } when targetType == typeof(Int64) => this.CastArray(array, Convert.ToInt64),
                { } when targetType == typeof(UInt16) => this.CastArray(array, Convert.ToUInt16),
                { } when targetType == typeof(UInt32) => this.CastArray(array, Convert.ToUInt32),
                { } when targetType == typeof(UInt64) => this.CastArray(array, Convert.ToUInt64),
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
                primitiveArray = (value as IPrimitiveArray);
                return true;
            }

            primitiveArray = null;
            return false;
        }

        private async Task ParseCommandScalarValueAsync(WriteRequestWrapper command, DataValue dataValue, Session session)
        {
            var (dataTypeId, builtInType) = this.GetBuiltInType(dataValue.Value);
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
                var structType = await this.LoadStructTypeAsync(dataTypeId).ConfigureAwait(false);
                var dictionary = values.ToDictionary(x => x.Key, y => y.Value as object);
                return this.BuildOpcUaStruct(structType, dictionary);
            }
            else
            {
                var dataTypeNode = session.NodeCache.Find(dataTypeId);
                throw new Exception($"Data type is not a valid struct: {dataTypeNode.DisplayName}");
            }
        }

        private async Task<Array> CreateOpcUaStructArrayAsync(WriteRequestWrapper command, NodeId dataTypeId)
        {
            var structType = await this.LoadStructTypeAsync(dataTypeId).ConfigureAwait(false);
            return this.BuildOpcUaStructArray(structType, this.UnboxStructArray(command.Value));
        }

        private async Task<Type> LoadStructTypeAsync(NodeId dataTypeId)
        {
            return await this.LoadTypeAsync(dataTypeId);
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
                        value = this.BuildOpcUaStructArray(propInfo.PropertyType.GetElementType(), this.UnboxStructArray(value));
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
                structArray.SetValue(this.BuildOpcUaStruct(structType, structs[i]), i);
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