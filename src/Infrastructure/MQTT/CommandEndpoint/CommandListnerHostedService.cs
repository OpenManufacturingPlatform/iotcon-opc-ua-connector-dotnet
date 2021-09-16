using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.Consumers;

namespace OMP.Connector.Infrastructure.MQTT.CommandEndpoint
{
    public class CommandListnerHostedService : IHostedService
    {
        private readonly IMqttCommndListner _listner;
        private readonly IMqttRequestHandler _requestHandler;
        private readonly ILogger<CommandListnerHostedService> _logger;

        public CommandListnerHostedService(IMqttCommndListner listner, IMqttRequestHandler requestHandler, ILogger<CommandListnerHostedService> logger)
        {
            this._listner = listner;
            this._requestHandler = requestHandler;
            this._logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this._listner.StartReceivingAsync();
            _logger.LogInformation($"{nameof(CommandListnerHostedService)} opened connection to mqtt broker");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this._listner.StopReceivingAsync();
            _logger.Information($"{nameof(CommandListnerHostedService)} closed connections");
        }

        private void RequestSourceOnOnErrorOccured(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.Exception, $"{nameof(CommandListnerHostedService)}: {e.Message} ");
        }

        private void RequestSourceOnOnMessageReceived(object sender, CommandRequest command)
        {
            _logger.LogTrace($"{nameof(CommandListnerHostedService)} received a message.");

            try
            {
                if (command != null)
                {
                    _requestHandler.OnMessageReceived(command);
                    _logger.Trace($"{nameof(CommandListnerHostedService)} passed request message to handler, RequestMessage.{nameof(command.Id)}: {command.Id}, {nameof(command.MetaData.CorrelationIds)}: {String.Join(", ", command.MetaData.CorrelationIds)}");
                }
                else
                {
                    _logger.LogError("CommandRequest message was null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CommandListnerHostedService)} {ex.GetMessage()}");
            }
        }
    }
}