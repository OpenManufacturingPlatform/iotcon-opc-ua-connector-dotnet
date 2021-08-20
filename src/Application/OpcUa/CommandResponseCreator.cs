using System;
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
using OMP.Connector.Domain.Schema.Messages;

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
                SenderIdentifier = new Participant()
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

        public static CommandResponse GetErrorResponseMessage(string schemaUrl, CommandResponse failedResponse)
            => GetErrorResponseMessage(schemaUrl, failedResponse.Id, failedResponse.MetaData, failedResponse.Payload.ResponseSource);

        public static CommandResponse GetErrorResponseMessage(string schemaUrl, CommandRequest requestMessage)
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
                SenderIdentifier = requestMessage.MetaData?.DestinationIdentifiers?.First(),
                TimeStamp = DateTime.UtcNow
            };
            var messageId = Guid.NewGuid().ToString();

            return GetErrorResponseMessage(schemaUrl, messageId, metaData, responseSource);
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