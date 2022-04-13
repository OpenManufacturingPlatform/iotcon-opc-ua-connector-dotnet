using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Enums;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint;
using OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint;
using OneOf;

namespace OMP.Connector.Infrastructure.Kafka
{
    public class KafkaMessageSender : IMessageSender//, IDisposable
    {
        private readonly ILogger<KafkaMessageSender> _logger;
        private readonly IResponseProducer _responseProducer;
        private readonly IConfigurationProducer _configurationProducer;
        private readonly ITelemetryProducer _telemetryProducer;
        private readonly ConnectorConfiguration _connectorConfiguration;

        private readonly string _responseEndpointTopic;
        private readonly string _configurationEndpointTopic;

        public KafkaMessageSender(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IProducerFactory producerFactory,
            ILogger<KafkaMessageSender> logger)
        {
            this._connectorConfiguration = connectorConfiguration.Value;
            this._logger = logger;
            this._responseProducer = producerFactory.CreateResponseProducer();
            this._configurationProducer = producerFactory.CreateConfigurationProducer();
            this._telemetryProducer = producerFactory.CreateTelemetryProducer();
            this._responseEndpointTopic = _connectorConfiguration?.Communication?.ResponseEndpoint?.GetConfig<KafkaConfig>()?.Topic;
            this._configurationEndpointTopic = _connectorConfiguration?.Persistence?.GetConfig<KafkaConfig>()?.Topic;

            var argumentExceptions = new List<ArgumentException>();

            if (_responseProducer is null)
                argumentExceptions.Add(new ArgumentException("Configuration for Response Communication is empty or invalid"));

            if (_configurationProducer is null)
                argumentExceptions.Add(new ArgumentException("Configuration for Config Persistence is empty or invalid"));

            if (_telemetryProducer is null)
                argumentExceptions.Add(new ArgumentException("Configuration for Telemetry Communication is empty or invalid"));

            if (string.IsNullOrWhiteSpace(_responseEndpointTopic))
                argumentExceptions.Add(new ArgumentException("Configuration for ResponseEndpoint Topic is empty or invalid"));

            if (string.IsNullOrWhiteSpace(_configurationEndpointTopic))
                argumentExceptions.Add(new ArgumentException("Configuration Topic setting is empty or invalid"));

            if (argumentExceptions.Any())
                throw new AggregateException("Invalid or missing configuration", argumentExceptions);
        }

        public async Task SendMessageToComConUpAsync(IEnumerable<CommandResponse> commandResponse, CommandRequest commandRequest = null)
        {
            foreach (var response in commandResponse)
                await SendMessageToComConUpAsync(response, commandRequest);
        }

        public async Task SendMessageToComConUpAsync(CommandResponse commandResponse, CommandRequest commandRequest = null)
        {
            var requestId = commandResponse.MetaData?.CorrelationIds?.LastOrDefault();
            requestId ??= Guid.Empty.ToString();

            var result = await _responseProducer.ProduceAsync(requestId, commandResponse);
            await SendErrorResponseWhenOriginalSizeTooLargeAsync(commandResponse, result);

            LogProduceResultMessage(result, EventTypes.SentTelemetryToBroker, requestId, ($"Send [{nameof(CommandResponse)}] to route: [{_responseEndpointTopic ?? "*"}]"));
        }

        public bool SendMessageToConfig(AppConfigDto configuration)
        {
            var result = _configurationProducer.ProduceAsync(_connectorConfiguration.ConnectorId, configuration).GetAwaiter().GetResult();
            LogProduceResultMessage(result, EventTypes.SentConfigToBroker, _connectorConfiguration.ConnectorId, $"Send [{nameof(AppConfigDto)}] to route: [{_configurationEndpointTopic ?? "*"}]");

            return result.Match(publishSucceeded => true,
                publishPartialSucceeded => true,
                publishedFailedMessageSizeTooLarge => false,
                producerError => false);
        }

        public async Task SendMessageToTelemetryAsync(SensorTelemetryMessage telemetry)
        {
            var result = await _telemetryProducer.ProduceAsync(telemetry.Id, telemetry);
            LogProduceResultMessage(result, EventTypes.SentTelemetryToBroker, telemetry.Id);
        }

        private Task SendErrorResponseWhenOriginalSizeTooLargeAsync(CommandResponse commandResponse,
            OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed> result)
        {
            if (result.Value is PublishedFailedMessageSizeTooLarge)
                return this.SendMessageToComConUpAsync(ConstructErrorResponse(commandResponse));

            return Task.CompletedTask;
        }

        private CommandResponse ConstructErrorResponse(CommandResponse failedResponse)
            => CommandResponseCreator.GetErrorResponseMessage(_connectorConfiguration.Communication.SchemaUrl, failedResponse);

        private void LogProduceResultMessage(OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed> result, EventTypes eventTypes, string requestId)
            => LogProduceResultMessage(result, eventTypes, requestId, string.Empty);

        private void LogProduceResultMessage(OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed> result, EventTypes eventTypes, string requestId, string message)
        {
            result.Switch(
                publishSucceeded => _logger.LogTrace($"Publish Succeeded: {message}"),
                publishPartialSucceeded => _logger.LogTrace($"Publish Partially Succeeded: {message}"),
                publishedFailedMessageSizeTooLarge => _logger.LogTrace($"Publish Failed due to message size being to large: {message} => {publishedFailedMessageSizeTooLarge.Error}", publishedFailedMessageSizeTooLarge),
                producerError => _logger.LogTrace($"Publish Failed: {message} => {producerError.Error}", producerError)
            );

            _logger.LogEvent(eventTypes, requestId);
        }
    }
}