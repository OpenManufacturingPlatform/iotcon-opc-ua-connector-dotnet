// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Enums;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using Opc.Ua;

namespace OMP.Connector.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger _logger;
        private readonly string _schemaUrl;
        private readonly ISessionPoolStateManager _sessionPoolManager;
        private readonly ISubscriptionProviderFactory _subscriptionProviderFactory;

        public SubscriptionService(
            ISessionPoolStateManager sessionPoolStateManager,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ISubscriptionProviderFactory subscriptionProviderFactory,
            ILogger<SubscriptionService> logger
            )
        {
            this._logger = logger;
            this._schemaUrl = connectorConfiguration.Value.Communication.SchemaUrl;
            this._sessionPoolManager = sessionPoolStateManager;
            this._subscriptionProviderFactory = subscriptionProviderFactory;
        }

        public async Task<CommandResponse> ExecuteAsync(CommandRequest requestMessage)
        {
            CommandResponse responseMessage = default;
            IEnumerable<ICommandResponse> commandResponses = default;
            try
            {
                commandResponses = await this.ProcessCommandsAsync(requestMessage);
                this._logger.Trace($"{nameof(SubscriptionService)} message subscription requests were processed. RequestMessage.{nameof(requestMessage.Id)}: {requestMessage.Id}");
            }
            catch (ServiceResultException ex)
            {
                responseMessage = CommandResponseCreator.GetErrorResponseMessage(this._schemaUrl, requestMessage);
                this._logger.Error(ex, $"Subscription Request Service Error: [Endpoint: {requestMessage.Payload.RequestTarget.EndpointUrl}]");
            }
            catch (Exception ex)
            {
                responseMessage = CommandResponseCreator.GetErrorResponseMessage(this._schemaUrl, requestMessage);
                this._logger.Error(ex, $"Subscription Request Error: [Endpoint: {requestMessage.Payload.RequestTarget.EndpointUrl}]");
            }

            responseMessage ??= CommandResponseCreator.GetCommandResponseMessage(this._schemaUrl, requestMessage, commandResponses);
            this._logger.Trace($"{nameof(SubscriptionService)} message for subscription commands response was created. RequestMessage.{nameof(requestMessage.Id)}:{requestMessage.Id} >> ResponseMessage.{nameof(responseMessage.Id)}:{responseMessage.Id}");

            return responseMessage;
        }

        private async Task<List<ICommandResponse>> ProcessCommandsAsync(CommandRequest requestMessage)
        {
            var opcSession = await this._sessionPoolManager.GetSessionAsync(requestMessage.Payload.RequestTarget.EndpointUrl, new CancellationToken());
            var commandResults = new List<ICommandResponse>();
            await opcSession.UseAsync(async (session, complexTypeSystem) =>
            {
                var requestCounter = 0;
                foreach (var request in requestMessage.Payload.Requests)
                {
                    var provider = this._subscriptionProviderFactory.GetProvider(request, TelemetryMessageMetadata.MapFrom(requestMessage));
                    if (provider == default)
                        throw new ApplicationException("Subscription command is not supported");

                    this._logger.LogEvent(EventTypes.SentRequestToOpcUa, requestMessage.Id);

                    var response = await provider.ExecuteAsync(session, complexTypeSystem);

                    this._logger.LogEvent(EventTypes.ReceivedResponseFromOpcUa, requestMessage.Id);

                    this._logger.Trace($"{nameof(SubscriptionService)} subscription request was processed. RequestMessage.{nameof(requestMessage.Id)}: {requestMessage.Id} >> Request[{requestCounter++}]");

                    if (response != default)
                        commandResults.Add(response);
                }
            });

            return commandResults;
        }
    }
}