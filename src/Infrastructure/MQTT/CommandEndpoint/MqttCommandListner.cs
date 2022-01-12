using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.Consumers;
using OMP.Connector.Infrastructure.MQTT.Serialization;

namespace OMP.Connector.Infrastructure.MQTT.CommandEndpoint
{
    public class MqttCommandListner : MqttBaseListnerPublisher<IMqttClient, MqttClientSettings>, IMqttCommndListner
    {
        private readonly IMqttRequestHandler _requestHandler;
        public event EventHandler<CommandRequest> OnMessageReceived;

        public MqttCommandListner(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMqttClientFactory mqttClientFactory,
            IMqttRequestHandler requestHandler,
            ISerializer serializer,
            ILogger<MqttCommandListner> logger)
            : base(mqttClientFactory.CreateClient(connectorConfiguration.Value.Communication.RequestEndpoint, connectorConfiguration.Value.Communication.Shared),
                 connectorConfiguration.Value.Communication.RequestEndpoint.GetConfig<MqttClientSettings>(),
                 serializer,
                 logger,
                 connectorConfiguration.Value.Communication.RequestEndpoint.GetConfig<MqttClientSettings>().AutoReconnectTimeInSeconds)
        {
            this.Client.OnMessageReceived += Client_OnMessageReceived;
            this._requestHandler = requestHandler;
        }

        private void Client_OnMessageReceived(object sender, MqttMessageEventArgs e)
        {
            var commandRequest = TryConvertMessageToRequest(e);
            this._requestHandler.OnMessageReceived(commandRequest).GetAwaiter().GetResult();
        }

        public Task StartReceivingAsync()
        {
            if (!this.EstablishConnection())
                this.StartAutoReconnect(this.AutoReconnectTime);

            return Task.CompletedTask;
        }

        public Task StopReceivingAsync()
        {
            this.DropConnection();
            return Task.CompletedTask;
        }

        protected override bool EstablishConnection()
        {
            if (base.EstablishConnection())
            {
                try
                {
                    var topics = this.MqttClientSettings.Topics;
                    if (topics.Any(t => string.IsNullOrEmpty(t.TopicName)))
                    {
                        this.Logger?.LogCritical($"Unable to subscribe. Invalid topic configuration{string.Join(",", topics.Select(t => $"'{t}'"))}");
                        return false;
                    }
                    
                    this.Logger?.LogInformation($"Subscribing to topics {string.Join(",", topics.Select(t => $"'{t}'"))}");
                    this.Client.Subscribe(topics.Select(t => t.TopicName).ToArray(), topics.Select(t => t.QosLevel).ToArray());
                    return true;
                }
                catch (Exception e)
                {
                    this.Logger?.LogError(e, "{exception} occured: {BrokerAddress}", e.GetType().Name, this.MqttClientSettings.BrokerAddress);
                    base.RaiseOnErrorOccurred(this, e);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private CommandRequest TryConvertMessageToRequest(MqttMessageEventArgs message)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(message.Message);
                return this.Serializer.Deserialize<CommandRequest>(payload);
            }
            catch (Exception e)
            {
                this.Logger?.LogError(e, $"Unable to deserialize MQTT message from Topic {message.Topic}: {e.GetMessage()}");
                return null;
            }
        }
    }
}