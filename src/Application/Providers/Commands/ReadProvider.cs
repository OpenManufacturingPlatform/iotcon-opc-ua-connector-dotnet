using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Providers.Commands.Base;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using Opc.Ua;
using ReadRequest =OMP.Connector.Domain.Schema.Request.Control.ReadRequest;
using ReadResponse =OMP.Connector.Domain.Schema.Responses.Control.ReadResponse;

namespace OMP.Connector.Application.Providers.Commands
{
    public class ReadProvider: CommandProvider<ReadRequest, ReadResponse>
    {
        private readonly int _batchSize;

        public ReadProvider(
            IEnumerable<ReadRequest> commands,
            IOpcSession opcSession,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMapper mapper,
            ILogger<ReadProvider> logger): base(commands, opcSession, mapper, logger)
        {
            this._batchSize = connectorConfiguration.Value.OpcUa.ReadBatchSize;
        }

        protected override async Task<IEnumerable<ReadResponse>> RunCommandAsync()
        {
            try
            {
                var readResults = new List<ReadResponse>();
                var nodeIdCommands = this.GetNodeIdCommands();
                var results = this.ReadNodeIds(nodeIdCommands.Select(cmd => cmd.NodeId));

                var cmdResults = nodeIdCommands.Zip(results);
                foreach (var ((nodeId, nodeIdCmd), result) in cmdResults)
                {
                    var readResult = this.ConstructResult(nodeIdCmd);
                    try
                    {
                        var dataTypeId = TypeInfo.GetDataTypeId(result.nodeValue);
                        var builtinType = TypeInfo.GetBuiltInType(dataTypeId);
                        var valueRank = TypeInfo.GetValueRank(result.nodeValue);

                        var elementType = await this.GetElementTypeIfAvailableAsync(nodeId, builtinType, valueRank, this.OpcSession);

                        readResult.Message = result.serviceResult.ToString();
                        readResult.Value = TelemetryConversion.ConvertToMeasurement(result.nodeValue, elementType, this.Mapper);
                        readResult.DataType = TelemetryConversion.GetDataTypeName(builtinType, valueRank, elementType, result.nodeValue) ?? string.Empty;
                        readResult.OpcUaCommandType = OpcUaCommandType.Read;
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex, $"Error occurred on reading node in ReadProvider: {nodeId}");
                        readResult.Message = ServiceResult.Create(StatusCodes.Bad, default, default).ToString();
                        readResult.Value = new SensorMeasurementString(ex.Message);
                        readResult.DataType = string.Empty;
                    }
                    readResults.Add(readResult);
                }
                return readResults;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred during a read operation of node(s) from the OPC UA server", ex);
            }
        }

        private async Task<string> GetElementTypeIfAvailableAsync(NodeId nodeId, BuiltInType type, int valueRank, IOpcSession opcSession)
        {
            if (TelemetryConversion.IsArray(valueRank) && type == BuiltInType.ExtensionObject)
            {
                var node = opcSession.GetNode<VariableNode>(nodeId);
                var nodeType = await opcSession.LoadTypeAsync(node?.DataType);
                return nodeType?.Name;
            }
            return null;
        }

        private IEnumerable<(object nodeValue, ServiceResult serviceResult)> ReadNodeIds(IEnumerable<NodeId> nodeIds)
        {
            var values = new List<object>();
            var errors = new List<ServiceResult>();
            
            this.OpcSession.ReadNodes(nodeIds.ToList(), this._batchSize, values, errors);

            var mergedLists = values.Zip(errors);
            return mergedLists;
        }
    }
}