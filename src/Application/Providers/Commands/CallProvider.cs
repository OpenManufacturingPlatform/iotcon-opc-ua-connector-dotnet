// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Providers.Commands.Base;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using Opc.Ua;
using CallRequest = OMP.Connector.Domain.Schema.Request.Control.CallRequest;
using CallResponse = OMP.Connector.Domain.Schema.Responses.Control.CallResponse;

namespace OMP.Connector.Application.Providers.Commands
{
    public class CallProvider : CommandProvider<CallRequest, CallResponse>
    {
        private string _connectedEndpointUrl = string.Empty;

        public CallProvider(IEnumerable<CallRequest> commands, IOpcSession opcSession, IMapper mapper, ILogger<CallProvider> logger)
            : base(commands, opcSession, mapper, logger) { }

        protected override async Task<IEnumerable<CallResponse>> RunCommandAsync()
        {
            var methodInfoList = await this.GetMethodInfoListAsync();
            var callMethodRequests = this.MapCommandsToCallMethodRequests(methodInfoList);

            var methodCallResults = await this.ExecuteMethodsAsync(callMethodRequests);

            var methodCallResponse = this.BuildMethodCallResponse(methodCallResults, methodInfoList);
            return methodCallResponse;
        }

        private IEnumerable<CallMethodRequest> MapCommandsToCallMethodRequests(List<OpcUaMethodInfo> methodInfoList)
        {
            var methodRequests = this.Mapper.Map<List<CallMethodRequest>>(this.Commands);
            this.UpdateCallMethodRequests(methodRequests, methodInfoList);
            return methodRequests;
        }

        private async Task<List<OpcUaMethodInfo>> GetMethodInfoListAsync()
        {
            var listMethodInfo = new List<OpcUaMethodInfo>();
            foreach (var command in this.Commands)
            {
                var methodInfo = new OpcUaMethodInfo();
                await this.OpcSession.UseAsync((session, complexTypeSystem) =>
                {
                    methodInfo.MethodId = command.NodeId;
                    var node = session.NodeCache.Find(command.NodeId, ReferenceTypes.HasComponent, true, false).FirstOrDefault();
                    if (node != default)
                    {
                        methodInfo.ObjectId = ExpandedNodeId.ToNodeId(node.NodeId, session.NodeCache.NamespaceUris);
                    }

                    ClientSessionUtilities.GetMethodArguments(session, command.NodeId, this.Logger, out var inputArgs, out var outputArgs);
                    methodInfo.InputArgs = inputArgs;
                    methodInfo.OutputArgs = outputArgs;

                });
                listMethodInfo.Add(methodInfo);
            }
            return listMethodInfo;
        }

        private void UpdateCallMethodRequests(IEnumerable<CallMethodRequest> callRequests, List<OpcUaMethodInfo> methodInfoList)
        {
            foreach (var callRequest in callRequests)
            {
                var methodInfo = methodInfoList.FirstOrDefault(info => callRequest.MethodId.Equals(info.MethodId));
                callRequest.ObjectId = methodInfo?.ObjectId;

                if (callRequest.InputArguments.Any())
                {
                    this.EnsureCorrectInputOrderAndType(callRequest, methodInfo?.InputArgs);
                }
            }
        }

        private async Task<CallMethodResultCollection> ExecuteMethodsAsync(IEnumerable<CallMethodRequest> callMethodRequests)
        {
            var callMethodRequestCollection = new CallMethodRequestCollection(callMethodRequests);

            CallMethodResultCollection callMethodResultCollection = default;
            await this.OpcSession.UseAsync((session, complexTypeSystem) =>
            {
                session.Call(default, callMethodRequestCollection, out callMethodResultCollection, out _);
                this._connectedEndpointUrl = session.Endpoint.EndpointUrl;
            });

            return callMethodResultCollection;
        }

        private IEnumerable<CallResponse> BuildMethodCallResponse(CallMethodResultCollection callMethodResult, IReadOnlyCollection<OpcUaMethodInfo> methodInfoList)
        {
            var callResponses = new List<CallResponse>();
            var commandAndMethodCallResultTuple = this.Commands.Zip(callMethodResult);

            foreach (var (command, methodResult) in commandAndMethodCallResultTuple)
            {
                var callResult = this.ConstructResult(command);
                callResult.Message = methodResult.StatusCode.ToString();

                var methodInfo = methodInfoList.FirstOrDefault(info => command.NodeId == info.MethodId);
                var outputArgsTuple = methodResult.OutputArguments.Zip(methodInfo?.OutputArgs);

                var outputArgs = new List<OutputArgument>();
                foreach (var (argValue, argInfo) in outputArgsTuple)
                {
                    outputArgs.Add(new OutputArgument
                    {
                        Key = argInfo.Name,
                        DataType = argValue.TypeInfo.ToString(),
                        Value = argValue.ToString()
                    });
                }

                callResult.Arguments = outputArgs;

                callResult.OpcUaCommandType = OpcUaCommandType.Call;
                callResponses.Add(callResult);

                this.Logger.Debug($"Executed {command.GetType().Name} on NodeId: [{command.NodeId}] and Endpoint: [{this._connectedEndpointUrl}] " +
                                        $"with {command.Arguments.Count()} Input Arguments and {callResult.Arguments?.Count()} Output Arguments");
            }

            return callResponses;
        }

        private void EnsureCorrectInputOrderAndType(CallMethodRequest callCommand, IEnumerable<Argument> inputArguments)
        {
            var argsJoinedOnName = from outer in inputArguments
                                   join inner in callCommand.InputArguments on outer.Name equals this.GetKeyValuePair(inner).Key
                                   select (this.GetKeyValuePair(inner).Value, outer);

            callCommand.InputArguments = ConvertInputArgumentTypes(argsJoinedOnName);
        }

        private Opc.Ua.KeyValuePair GetKeyValuePair(Variant variant)
        {
            if (variant.Value is ExtensionObject { Body: Opc.Ua.KeyValuePair kvp })
                return kvp;

            throw new Exception("Expected Opc.Ua.Variant with KeyValuePair value.");
        }

        private static VariantCollection ConvertInputArgumentTypes(IEnumerable<(Variant, Argument)> variantInputArgumentTuple)
        {
            var convertedInputs = new VariantCollection();
            foreach (var (input, argument) in variantInputArgumentTuple)
            {
                var buildInType = TypeInfo.GetBuiltInType(argument.DataType);
                var converted = new Variant(TypeInfo.Cast(input, buildInType));
                convertedInputs.Add(converted);
            }
            return convertedInputs;
        }
    }
}