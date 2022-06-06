// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.AlarmSubscription.Base;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;
using OMP.Connector.Domain.Schema.Responses.AlarmSubscription;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.AlarmSubscription
{
    public class RespondToAlarmEventsProvider : AlarmSubscriptionProvider<RespondToAlarmEventsRequest, RespondToAlarmEventsResponse>
    {
        public RespondToAlarmEventsProvider(
            ILogger<RespondToAlarmEventsProvider> logger,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            RespondToAlarmEventsRequest command) : base(command, connectorConfiguration, logger){}

        protected override async Task<string> ExecuteCommandAsync()
        {
            var results = new List<string>();
            foreach(var eventAction in this.Command.AlarmEventActions)
            {
                var methodNodeId = eventAction.Action switch
                {
                    "Acknowledge" => MethodIds.AcknowledgeableConditionType_Acknowledge,
                    "Confirm" => MethodIds.AcknowledgeableConditionType_Confirm,
                    _ => null
                };
                var statusCode = CallMethod(eventAction.SourceNodeId, methodNodeId, eventAction.Comment, eventAction.EventId);
                if(StatusCode.IsBad(statusCode))
                    results.Add(GetBadCallResult(statusCode, eventAction.Action, eventAction.Comment));
            }

            this.Logger.Debug($"Created/Updated alarm subscriptions on Endpoint: [{this.EndpointUrl}]");

            return this.GetStatusMessage(results);
        }

        private string GetBadCallResult(StatusCode statusCode, string eventAction, string comment)
        {
            return $"{statusCode}: Error when trying to respond to event: Action: {eventAction} Comment: {comment}";
        }

        protected override void GenerateResult(RespondToAlarmEventsResponse result, string message)
        {
            result.OpcUaCommandType = OpcUaCommandType.RespondToAlarmEvents;
            result.Message = message;
        }

        private StatusCode CallMethod(NodeId sourceNodeId, NodeId methodId, string comment, byte[] eventId)
        {
            var methodsToCall = new CallMethodRequestCollection();

            var request = new CallMethodRequest
            {
                ObjectId = sourceNodeId,
                MethodId = methodId
            };

            if (comment != null)
            {
                request.InputArguments.Add(new Variant(eventId));
                request.InputArguments.Add(new Variant((LocalizedText)comment));
            }

            methodsToCall.Add(request);

            Session.Call(
                null,
                methodsToCall,
                out CallMethodResultCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, methodsToCall);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, methodsToCall);

            return results.First().StatusCode;
        }
    }
}
