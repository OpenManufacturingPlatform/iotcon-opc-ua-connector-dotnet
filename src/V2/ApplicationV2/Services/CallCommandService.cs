// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Services
{
    internal sealed class CallCommandService : ICallCommandService
    {
        public Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(IOpcUaSession opcUaSession, NodeMethodDescribeCommand command, CancellationToken cancellationToken)
            => opcUaSession.GetNodeMethodArgumentsAsync(command, cancellationToken);

        public async Task<CallCommandCollectionResponse> CallNodesAsync(IOpcUaSession opcUaSession, CallCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var callMethodRequests = new List<CallMethodRequest>();
                foreach (var cmd in commands)
                {
                    var methodInfo = await opcUaSession.GetNodeMethodArgumentsAsync(cmd.NodeId, cancellationToken);
                    var callMethodRequest = CreateCallMethodRequest(cmd, methodInfo);

                    UpdatenputArgumentTypes(cmd, methodInfo, ref callMethodRequest);

                    callMethodRequests.Add(callMethodRequest);
                }

                var callResponse = await opcUaSession.CallAsync(callMethodRequests, cancellationToken);
                
                if(callResponse.Results.All(callResponse => StatusCode.IsGood(callResponse.StatusCode)))
                    return CallCommandCollectionResponse.Success(commands, callResponse);
                
                return CallCommandCollectionResponse.Failed(commands, callResponse);
            }
            catch (Exception ex)
            {
                return CallCommandCollectionResponse.Failed(commands, ex);
            }
        }

        private static CallMethodRequest CreateCallMethodRequest(CallCommand cmd, NodeMethodDescribeResponse methodInfo)
            => new CallMethodRequest
            {
                ObjectId = methodInfo.ObjectId,
                MethodId = methodInfo.MethodId,
                InputArguments = new VariantCollection(cmd.Arguments.Select(arg => new Variant(new Opc.Ua.KeyValuePair() { Key = arg.Key, Value = new Variant(arg.Value) })))
            };

        private static void UpdatenputArgumentTypes(CallCommand callCommand, NodeMethodDescribeResponse methodInfo, ref CallMethodRequest callMethodRequest)
        {
            if (callCommand.Arguments.Any())
            {
                var argsJoinedOnName = from outer in methodInfo.InputArguments
                                       join inner in callMethodRequest.InputArguments on outer.Name equals GetKeyValuePair(inner).Key
                                       select (GetKeyValuePair(inner).Value, outer);

                callMethodRequest.InputArguments = ConvertInputArgumentTypes(argsJoinedOnName);
            }
        }

        private static Opc.Ua.KeyValuePair GetKeyValuePair(Variant variant)
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
