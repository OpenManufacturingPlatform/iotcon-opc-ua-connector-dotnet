// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Enums;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;
using OMP.Connector.Domain.Schema.Request.Control.Base;
using OMP.Connector.Domain.Schema.Request.Discovery.Base;
using OMP.Connector.Domain.Schema.Request.Subscription.Base;

namespace OMP.Connector.Application.Clients.Base
{
    public abstract class RequestHandler
    {
        protected readonly IMessageSender MessageSender;
        protected readonly ILogger Logger;
        protected readonly IMapper Mapper;
        protected readonly ICommandService CommandService;
        protected readonly ISubscriptionServiceStateManager SubscriptionServiceStateManager;
        protected readonly IAlarmSubscriptionServiceStateManager AlarmSubscriptionServiceStateManager;
        protected readonly IDiscoveryService DiscoveryService;
        private readonly ConnectorConfiguration _connectorConfiguration;
        protected readonly AbstractValidator<CommandRequest> RequestValidator;
        protected string SchemaUrl => this._connectorConfiguration.Communication.SchemaUrl;

        protected RequestHandler(
            ILogger logger,
            IMessageSender messageSender,
            IMapper mapper,
            ICommandService commandService,
            ISubscriptionServiceStateManager subscriptionServiceStateManager,
            IAlarmSubscriptionServiceStateManager alarmSubscriptionServiceStateManager,
            IDiscoveryService discoveryService,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            AbstractValidator<CommandRequest> commandRequestValidator)
        {
            this.Logger = logger;
            this.MessageSender = messageSender;
            this.Mapper = mapper;
            this.CommandService = commandService;
            this.SubscriptionServiceStateManager = subscriptionServiceStateManager;
            this.AlarmSubscriptionServiceStateManager = alarmSubscriptionServiceStateManager;
            this.DiscoveryService = discoveryService;
            this._connectorConfiguration = connectorConfiguration.Value;
            this.RequestValidator = commandRequestValidator;
        }

        public virtual async Task OnMessageReceivedAsync(CommandRequest commandMessage)
        {
            try
            {
                this.Logger.LogEvent(EventTypes.ReceivedRequestFromBroker, commandMessage.Id);
                var responses = await this.ProcessCommand(commandMessage);
                await this.MessageSender.SendMessageToComConUpAsync(responses, commandMessage);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, $"{nameof(RequestHandler)} could not process request message. Id: {commandMessage.Id}");
                var requestNotProcessedResponse = CommandResponseCreator.GetCommandResponseMessage(this.SchemaUrl, commandMessage, null);
                await this.MessageSender.SendMessageToComConUpAsync(requestNotProcessedResponse, commandMessage);
            }
        }

        protected virtual async Task<IEnumerable<CommandResponse>> ProcessCommand(CommandRequest commandMessage)
        {
            if (this._connectorConfiguration.EnableMessageFilter)
            {
                this.Logger.Trace($"{nameof(RequestHandler)} started filtering of request. RequestMessage.{nameof(commandMessage.Id)}: {commandMessage.Id}");
                if (this.IsInvalidRequest(commandMessage))
                    return Array.Empty<CommandResponse>();
            }

            this.Logger.Debug($"{nameof(RequestHandler)} started processing of request. RequestMessage.{nameof(commandMessage.Id)}: {commandMessage.Id}");

            var commandRequests = commandMessage.Payload.Requests.ToList();
            if (!commandRequests.Any())
            {
                var emptyRequestResponse = CommandResponseCreator.GetCommandResponseMessage(this.SchemaUrl, commandMessage, null);
                return new CommandResponse[] { emptyRequestResponse };
            }

            var opcUaCommands = new List<ICommandRequest>();
            var subscriptionCommands = new List<ICommandRequest>();
            var alarmSubscriptionCommands = new List<ICommandRequest>();
            var discoveryCommands = new List<ICommandRequest>();

            foreach (var req in commandRequests)
            {
                switch (req)
                {
                    case NodeCommandRequest opcUaCommand:
                        opcUaCommands.Add(opcUaCommand);
                        break;
                    case SubscriptionRequest subscriptionRequest:
                        subscriptionCommands.Add(subscriptionRequest);
                        break;
                    case AlarmSubscriptionRequest alarmSubscriptionRequest:
                        alarmSubscriptionCommands.Add(alarmSubscriptionRequest);
                        break;
                    case DiscoveryRequest discoveryRequest:
                        discoveryCommands.Add(discoveryRequest);
                        break;
                    default:
                        throw new ArgumentException("Message format not supported.", nameof(req.GetType));
                }
            }

            //TODO: Consider a Task.WaitAll for better parallelism

            var commandRequestResponse = await this.ProcessCommandRequestsAsync(opcUaCommands, commandMessage);
            var subscriptionCommandResponse = await this.ProcessSubscriptionRequestsAsync(subscriptionCommands, commandMessage);
            var alarmSubscriptionCommandResponse = await this.ProcessAlarmSubscriptionRequestsAsync(alarmSubscriptionCommands, commandMessage);
            var discoveryCommandResponse = await this.ProcessDiscoveryRequestsAsync(discoveryCommands, commandMessage);

            var responses = new List<CommandResponse>();

            if (commandRequestResponse is not null)
                responses.Add(commandRequestResponse);

            if (subscriptionCommandResponse is not null)
                responses.Add(subscriptionCommandResponse);

            if (alarmSubscriptionCommandResponse is not null)
                responses.Add(alarmSubscriptionCommandResponse);

            if (discoveryCommandResponse is not null)
                responses.Add(discoveryCommandResponse);

            this.Logger.Debug($"{nameof(RequestHandler)} finished processing of request. RequestMessage.{nameof(commandMessage.Id)}: {commandMessage.Id}");

            return responses;
        }

        private bool IsInvalidRequest(CommandRequest commandMessage)
        {
            var validationResult = this.RequestValidator.Validate(commandMessage);
            var requestIsInvalid = validationResult is { IsValid: false };

            if (!requestIsInvalid)
                return false;

            var reason = validationResult?.ToString(";");
            this.Logger.Debug($"{nameof(RequestHandler)} RequestMessage.{nameof(commandMessage.Id)}: {commandMessage.Id} was discarded because of: {reason}");
            return true;
        }

        protected virtual async Task<CommandResponse?> ProcessCommandRequestsAsync(IReadOnlyCollection<ICommandRequest> filteredRequests, CommandRequest originalRequest)
        {
            if (!filteredRequests.Any())
                return null;

            CommandResponse response;
            try
            {
                var commandRequest = this.CloneRequest(filteredRequests, originalRequest);
                response = await this.CommandService.ExecuteAsync(commandRequest);
            }
            catch (Exception exception)
            {
                response = CommandResponseCreator.GetErrorResponseMessage(this.SchemaUrl, originalRequest);
                this.Logger.Error(exception);
            }

            return response;
        }

        protected virtual async Task<CommandResponse?> ProcessSubscriptionRequestsAsync(IReadOnlyCollection<ICommandRequest> filteredRequests, CommandRequest originalRequest)
        {
            if (!filteredRequests.Any())
                return null;

            try
            {
                CommandResponse response;
                try
                {
                    var subscriptionRequest = this.CloneRequest(filteredRequests, originalRequest);
                    var opcUaServerUrl = originalRequest.Payload.RequestTarget.EndpointUrl;
                    var subscriptionService = this.SubscriptionServiceStateManager.GetSubscriptionServiceInstanceAsync(opcUaServerUrl, CancellationToken.None).GetAwaiter().GetResult();

                    response = await subscriptionService.ExecuteAsync(subscriptionRequest);
                }
                catch (Exception exception)
                {
                    response = CommandResponseCreator.GetErrorResponseMessage(this.SchemaUrl, originalRequest);
                    this.Logger.Error(exception);
                }

                return response;
            }
            finally
            {
                await this.CleanupStaleServices();
            }
        }

        protected virtual async Task<CommandResponse?> ProcessAlarmSubscriptionRequestsAsync(IReadOnlyCollection<ICommandRequest> filteredRequests, CommandRequest originalRequest)
        {
            if (!filteredRequests.Any())
                return null;

            try
            {
                CommandResponse response;
                try
                {
                    var subscriptionRequest = this.CloneRequest(filteredRequests, originalRequest);
                    var opcUaServerUrl = originalRequest.Payload.RequestTarget.EndpointUrl;
                    var alarmSubscriptionService = this.AlarmSubscriptionServiceStateManager.GetAlarmSubscriptionServiceInstanceAsync(opcUaServerUrl, CancellationToken.None).GetAwaiter().GetResult();

                    response = await alarmSubscriptionService.ExecuteAsync(subscriptionRequest);
                }
                catch (Exception exception)
                {
                    response = CommandResponseCreator.GetErrorResponseMessage(this.SchemaUrl, originalRequest);
                    this.Logger.Error(exception);
                }

                return response;
            }
            finally
            {
                await this.CleanupStaleServices();
            }
        }

        protected virtual async Task<CommandResponse?> ProcessDiscoveryRequestsAsync(IReadOnlyCollection<ICommandRequest> filteredRequests, CommandRequest originalRequest)
        {
            if (!filteredRequests.Any())
                return null;

            CommandResponse response;
            try
            {
                var discoveryRequest = this.CloneRequest(filteredRequests, originalRequest);
                response = await this.DiscoveryService.ExecuteAsync(discoveryRequest);
            }
            catch (Exception exception)
            {
                response = CommandResponseCreator.GetErrorResponseMessage(this.SchemaUrl, originalRequest);
                this.Logger.Error(exception);
            }

            return response;
        }

        protected virtual async Task CleanupStaleServices()
        {
            try
            {
                await this.SubscriptionServiceStateManager.CleanupStaleServicesAsync();
            }
            catch
            {
                this.Logger.Warning("Error occurred during cleanup of stale subscription services.");
            }
        }

        protected virtual CommandRequest CloneRequest(IEnumerable<ICommandRequest> filteredRequests, CommandRequest originalRequest)
        {
            var clonedRequestMessage = this.Mapper.Map<CommandRequest>(originalRequest);
            var commandRequests = filteredRequests as ICommandRequest[] ?? filteredRequests.ToArray();
            if (commandRequests.Any())
                clonedRequestMessage.Payload.Requests = commandRequests;

            return clonedRequestMessage;
        }
    }
}
