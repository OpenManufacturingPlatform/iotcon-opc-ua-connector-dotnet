using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Clients.Base;
using OMP.Connector.Application.Services;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Infrastructure.MQTT
{
    public class MqttRequestHandler : RequestHandler, IMqttRequestHandler
    {
        public MqttRequestHandler(
            ILogger logger,
            IMessageSender messageSender,
            IMapper mapper,
            ICommandService commandService,
            ISubscriptionServiceStateManager subscriptionServiceStateManager,
            IDiscoveryService discoveryService,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            CommandRequestValidator commandRequestValidator)
            : base(logger, messageSender, mapper, commandService, subscriptionServiceStateManager, discoveryService,
                connectorConfiguration, commandRequestValidator)
        { }

        public Task OnMessageReceived(CommandRequest request)
            => OnMessageReceivedAsync(request);

    }
}