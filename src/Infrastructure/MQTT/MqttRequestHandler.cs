// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Clients.Base;
using OMP.Connector.Application.Services;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Infrastructure.MQTT
{
    public class MqttRequestHandler : RequestHandler, IMqttRequestHandler
    {
        public MqttRequestHandler(
            ILogger<MqttRequestHandler> logger,
            IMessageSender messageSender,
            IMapper mapper,
            ICommandService commandService,
            ISubscriptionServiceStateManager subscriptionServiceStateManager,
            IAlarmSubscriptionServiceStateManager alarmSubscriptionServiceStateManager,
            IDiscoveryService discoveryService,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            AbstractValidator<CommandRequest> commandRequestValidator)
            : base(logger, messageSender, mapper, commandService, subscriptionServiceStateManager, alarmSubscriptionServiceStateManager, discoveryService,
                connectorConfiguration, commandRequestValidator)
        { }

        public Task OnMessageReceived(CommandRequest request)
            => OnMessageReceivedAsync(request);

    }
}
