﻿using System;
using System.Collections.Generic;
using System.Linq;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.MetaData.Message;
using OMP.Connector.Domain.Schema.Responses;

namespace OMP.Connector.Application.OpcUa
{
    public static class CommandResponseCreator
    {
        public static CommandResponse GetCommandResponseMessage(
            string schemaUrl,
            CommandRequest commandRequest,
            IEnumerable<ICommandResponse> commandResponses)
        {
            if (string.IsNullOrWhiteSpace(schemaUrl))
                throw new ArgumentNullException(nameof(schemaUrl));

            if (commandRequest is null)
                throw new ArgumentNullException(nameof(commandRequest));

            var correlationIds = new List<string>(commandRequest.MetaData?.CorrelationIds ?? Array.Empty<string>());
            var responseMessage = ModelFactory.CreateInstance<CommandResponse>(schemaUrl);
            responseMessage.Id = Guid.NewGuid().ToString();
            var destinationToSender = commandRequest.MetaData?.DestinationIdentifiers?.FirstOrDefault();
            responseMessage.MetaData = new MessageMetaData()
            {
                TimeStamp = DateTime.UtcNow,
                SenderIdentifier = destinationToSender is null ? default : new Participant()
                {
                    Id = destinationToSender?.Id,
                    Name = destinationToSender?.Name,
                    Route = destinationToSender?.Route,
                    Type = ParticipantType.Gateway
                },
                DestinationIdentifiers = new List<Participant>() { commandRequest.MetaData?.SenderIdentifier },
                CorrelationIds = correlationIds
            };
            responseMessage.Payload = new ResponsePayload()
            {
                ResponseStatus = GetResponseStatus(commandResponses),
                ResponseSource = new ResponseSource()
                {
                    Id = string.Empty,
                    Name = string.Empty,
                    Route = string.Empty,
                    EndpointUrl = commandRequest.Payload.RequestTarget.EndpointUrl
                },
                Responses = commandResponses ?? new List<ICommandResponse>()
            };
            return responseMessage;
        }

        public static CommandResponse GetNullRequestErrorResponseMessage()
            => GetErrorResponseMessage("http://No-Schema-found.json", "Unknown RequestId", GetMessageMetaDataForNullRequestMessage(), GetResponseSourceForNullRequestMessage());

        public static CommandResponse GetErrorResponseMessage(string schemaUrl, CommandResponse failedResponse)
            => GetErrorResponseMessage(schemaUrl, failedResponse.Id, failedResponse.MetaData, failedResponse.Payload.ResponseSource);

        public static CommandResponse GetErrorResponseMessage(string schemaUrl, CommandRequest requestMessage, string errorMessage = null)
        {
            var responseSource = new ResponseSource()
            {
                EndpointUrl = requestMessage.Payload.RequestTarget.EndpointUrl,
                Id = string.Empty,
                Name = string.Empty,
                Route = string.Empty
            };

            var metaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>() { requestMessage.Id },
                DestinationIdentifiers = new List<Participant>() { requestMessage.MetaData?.SenderIdentifier },
                SenderIdentifier = requestMessage.MetaData?.DestinationIdentifiers?.FirstOrDefault(),
                TimeStamp = DateTime.UtcNow
            };
            var messageId = Guid.NewGuid().ToString();

            return GetErrorResponseMessage(requestMessage, schemaUrl, messageId, metaData, responseSource, errorMessage);
        }

        private static CommandResponse GetErrorResponseMessage(string schemaUrl, string messageId, MessageMetaData metaData, ResponseSource responseSource)
        {
            var message = ModelFactory.CreateInstance<CommandResponse>(schemaUrl);

            message.Id = messageId;
            message.MetaData = metaData;
            message.Payload = new ResponsePayload
            {
                ResponseStatus = OpcUaResponseStatus.Bad,
                ResponseSource = responseSource,
                Responses = new List<ICommandResponse>()
            };
            return message;
        }

        private static CommandResponse GetErrorResponseMessage(CommandRequest requestMessage, string schemaUrl, string messageId, MessageMetaData metaData, ResponseSource responseSource, string errorMessage = null)
        {
            var message = ModelFactory.CreateInstance<CommandResponse>(schemaUrl);
            var commandTypePropertyName = nameof(Domain.Schema.Request.Base.CommandRequest.OpcUaCommandType);
            var requestResponses = ExtractErrorResponsesFromRequest(requestMessage, responseSource, commandTypePropertyName, errorMessage);

            message.Id = messageId;
            message.MetaData = metaData;
            message.Payload = new ResponsePayload
            {
                ResponseStatus = OpcUaResponseStatus.Bad,
                ResponseSource = responseSource,
                Responses = requestResponses ?? new List<ICommandResponse>()
            };
            return message;
        }

        private static List<ICommandResponse>? ExtractErrorResponsesFromRequest(CommandRequest requestMessage, ResponseSource responseSource, string commandTypePropertyName, string errorMessage = null)
        {
            return requestMessage.Payload?.Requests?.Select(req => new Domain.Schema.Responses.Base.CommandResponse
            {
                OpcUaCommandType = (OpcUaCommandType)Convert.ChangeType(req.GetType().GetProperty(commandTypePropertyName).GetValue(req), typeof(OpcUaCommandType)),
                ErrorSource = responseSource.EndpointUrl,
                Message = errorMessage ?? "Unknown error",
                StatusCode = nameof(OpcUaResponseStatus.Bad)
            }).Cast<ICommandResponse>().ToList();
        }

        public static ResponseSource GetResponseSourceForNullRequestMessage()
            => new ResponseSource()
            {
                EndpointUrl = "http://endpoint-unknonw-due-to-null-request",
                Id = string.Empty,
                Name = string.Empty,
                Route = string.Empty
            };
        public static MessageMetaData GetMessageMetaDataForNullRequestMessage()
            => new MessageMetaData()
            {
                CorrelationIds = new List<string>(),
                DestinationIdentifiers = new List<Participant>(),
                SenderIdentifier = new Participant
                {
                    Id = string.Empty,
                    Name = string.Empty,
                    Route = string.Empty,
                    Type = ParticipantType.Application
                },
                TimeStamp = DateTime.UtcNow
            };

        private static OpcUaResponseStatus GetResponseStatus(IEnumerable<ICommandResponse> commandResponses)
        {
            if (commandResponses is null || !commandResponses.Any()) return OpcUaResponseStatus.Bad;
            return commandResponses.All(response =>
                (response as OMP.Connector.Domain.Schema.Responses.Base.CommandResponse).StatusIsGood())
                ? OpcUaResponseStatus.Good
                : OpcUaResponseStatus.Bad;
        }
    }
}