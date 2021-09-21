using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application.Providers.Commands.Base;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Enums;
using Opc.Ua;
using WriteRequest = OMP.Connector.Domain.Schema.Request.Control.WriteRequest;
using WriteResponse = OMP.Connector.Domain.Schema.Responses.Control.WriteResponse;

namespace OMP.Connector.Application.Providers.Commands
{
    public class WriteProvider : CommandProvider<WriteRequest, WriteResponse>
    {
        public WriteProvider(IEnumerable<WriteRequest> commands, IOpcSession opcSession, IMapper mapper, ILogger<WriteProvider> logger)
            : base(commands, opcSession, mapper, logger) { }

        protected override async Task<IEnumerable<WriteResponse>> RunCommandAsync()
        {
            var writeResults = new List<WriteResponse>();
            try
            {
                var nodeIdCommands = this.GetNodeIdCommands();
                var writeValues = this.ConvertToWriteValues(nodeIdCommands);

                var statusCodeCollection = this.WriteNodeIds(writeValues.ToList());
                writeResults = nodeIdCommands.Zip(statusCodeCollection, (value, code)
                    => this.GenerateResponse(value.NodeId, value.Command, code.ToString())).ToList();
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"An error occurred during the write command: {ex.Message}");
            }
            return await Task.FromResult(writeResults);
        }

        private WriteResponse GenerateResponse(
            NodeId nodeId,
            WriteRequest command,
            string status)
        {
            var writeResult = this.ConstructResult(command);
            writeResult.Message = status;
            writeResult.OpcUaCommandType = OpcUaCommandType.Write;

            this.Logger.Debug($"Executed {this.Commands.FirstOrDefault()?.GetType().Name} on NodeId: [{nodeId}] with value: [{command.Value}]");

            return writeResult;
        }

        private IEnumerable<WriteValue> ConvertToWriteValues(IEnumerable<(NodeId, WriteRequest)> nodeIdCommands)
        {
            var writeRequestWrappers = this.ConvertToWriteRequestWrappers(nodeIdCommands);
            this.ConvertToOpcUaTypedValues(writeRequestWrappers);
            return this.Mapper.Map<List<WriteValue>>(writeRequestWrappers);
        }

        private List<WriteRequestWrapper> ConvertToWriteRequestWrappers(IEnumerable<(NodeId, WriteRequest)> nodeIdCommands)
        {
            var writeRequestWrappers = new List<WriteRequestWrapper>();
            foreach (var (nodeId, writeCommand) in nodeIdCommands)
            {
                var command = this.Mapper.Map<WriteRequestWrapper>(writeCommand);
                command.RegisteredNodeId = nodeId.ToString();
                writeRequestWrappers.Add(command);
            }
            return writeRequestWrappers;
        }

        private StatusCodeCollection WriteNodeIds(List<WriteValue> writeValues)
        {
            var writeValueCollection = new WriteValueCollection(writeValues);
            var statusCodeCollection = this.OpcSession.WriteNodes(writeValueCollection);
            return statusCodeCollection;
        }

        public void ConvertToOpcUaTypedValues(IEnumerable<WriteRequestWrapper> writeRequests)
            => this.OpcSession.ConvertToOpcUaTypedValues(writeRequests);
    }
}