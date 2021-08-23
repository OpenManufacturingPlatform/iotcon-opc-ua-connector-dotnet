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
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Application.Services
{
    public class CommandService : ICommandService
    {
        private readonly ILogger _logger;
        private readonly ISessionPoolStateManager _sessionPoolManager;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly ICommandProviderFactory _commandProcessorFactory;

        public CommandService(
            ICommandProviderFactory commandProcessorFactory,
            ISessionPoolStateManager sessionPoolStateManager,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILogger<CommandService> logger
            )
        {
            this._commandProcessorFactory = commandProcessorFactory;
            this._sessionPoolManager = sessionPoolStateManager;
            this._connectorConfiguration = connectorConfiguration.Value;
            this._logger = logger;
        }

        public async Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest)
        {
            CommandResponse responseMessage = default;
            IEnumerable<ICommandResponse> commandResponses = default;

            try
            {
                this._logger.LogEvent(EventTypes.SentRequestToOpcUa, commandRequest.Id);

                commandResponses = await this.ProcessCommandsAsync(commandRequest);

                this._logger.LogEvent(EventTypes.ReceivedResponseFromOpcUa, commandRequest.Id);

                this._logger.Trace($"{nameof(CommandService)} message command requests were processed. RequestMessage.{nameof(commandRequest.Id)}: {commandRequest.Id}");
            }
            catch (Exception ex)
            {
                responseMessage = CommandResponseCreator.GetErrorResponseMessage(this._connectorConfiguration.Communication.SchemaUrl, commandRequest);
                var errorMessage = BuildErrorMessage(commandRequest, ex);
                this._logger.Error(errorMessage);
            }

            responseMessage ??= CommandResponseCreator.GetCommandResponseMessage(this._connectorConfiguration.Communication.SchemaUrl, commandRequest, commandResponses);

            this._logger.Trace($"{nameof(CommandService)} message for command response was created. RequestMessage.{nameof(commandRequest.Id)}:{commandRequest.Id} >> ResponseMessage.{nameof(responseMessage.Id)}:{responseMessage.Id}");

            return responseMessage;
        }

        private static string BuildErrorMessage(CommandRequest commandRequest, Exception ex)
        {
            var errorMessage = $"{nameof(CommandService)} error occurred for a request message.";
            errorMessage +=
                $" RequestMessage.{nameof(commandRequest.Id)}: {commandRequest.Id}, {nameof(commandRequest.Payload.RequestTarget.EndpointUrl)}: {commandRequest.Payload.RequestTarget.EndpointUrl}";
            errorMessage += $", ErrorMessage: {ex.GetMessage()}";
            return errorMessage;
        }

        private async Task<IEnumerable<ICommandResponse>> ProcessCommandsAsync(CommandRequest commandRequest)
        {
            var commands = commandRequest.Payload.Requests;
            var opcSession = await this._sessionPoolManager.GetSessionAsync(commandRequest.Payload.RequestTarget.EndpointUrl, new CancellationToken());
            var commandResults = new List<ICommandResponse>();
            var processors = this._commandProcessorFactory.GetProcessors(commands, opcSession);

            foreach (var processor in processors)
            {
                var results = await processor.ExecuteAsync();

                if (Equals(results, default))
                    continue;

                commandResults.AddRange(results);
            }

            return commandResults;
        }
    }
}