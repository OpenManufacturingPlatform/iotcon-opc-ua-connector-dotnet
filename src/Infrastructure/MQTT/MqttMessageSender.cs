// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Infrastructure.MQTT.Common.Publishers;

namespace OMP.Connector.Infrastructure.MQTT
{
    public class MqttMessageSender: IMessageSender
    {
        private readonly IMqttResponsePublisher _responsePublisher;
        private readonly IMqttTelemetryPublisher _telemetryPublisher;
        private readonly ILogger<MqttMessageSender> _logger;

        public MqttMessageSender(
            IMqttResponsePublisher responsePublisher,
            IMqttTelemetryPublisher telemetryPublisher,
            ILogger<MqttMessageSender> logger
            )
        {
            this._responsePublisher = responsePublisher;
            this._telemetryPublisher = telemetryPublisher;
            this._logger = logger;
        }

        public async Task SendMessageToComConUpAsync(IEnumerable<CommandResponse> commandResponse, CommandRequest commandRequest = null)
        {
            foreach (var response in commandResponse)
                await SendMessageToComConUpAsync(response, commandRequest);
        }

        public async Task SendMessageToComConUpAsync(CommandResponse commandResponse, CommandRequest commandRequest = null)
        {
            await _responsePublisher.PublishAsync(commandResponse);
            _logger.LogTrace($"{nameof(MqttMessageSender)} send message to ComConUp topic. ResponseMessage.{nameof(commandResponse.Id)}: {commandResponse.Id}");
        }
        public async Task SendMessageToTelemetryAsync(SensorTelemetryMessage telemetry)
        {
            await _telemetryPublisher.PublishAsync(telemetry);
            _logger.LogTrace($"{nameof(MqttMessageSender)} send message to telemetry topic.");
        }

        public bool SendMessageToConfig(AppConfigDto configuration)
        {
            throw new NotSupportedException();
        }
    }
}