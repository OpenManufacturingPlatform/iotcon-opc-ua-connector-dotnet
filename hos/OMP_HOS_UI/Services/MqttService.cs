using System;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace OmpHandsOnUi.Services
{
    public class MqttService : IDisposable
    {
        private ConfigurationModel configuration;
        private MqttClient? _mqttClient;
        private readonly byte _QosLevel = 1;
        private readonly ConfigurationService configurationService;
        private bool _serviceStarted;
        private bool _startInitialized;
        private object _lock = new object();
        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        public event EventHandler<ResponseMessage>? OnResponseMessage;
        public event EventHandler<TelemetryMessage>? OnTelemetryMessage;

        public MqttService(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.configurationService.OnConfigurationChange += ConfigurationService_OnConfigurationChange;
            this.configuration = configurationService.GetConfiguration();
            this.InitializeFromConfig();
        }

        private void ConfigurationService_OnConfigurationChange(object? sender, ConfigurationModel newConfiguration)
        {
            lock (_lock)
            {
                if (AreTheSame(this.configuration, newConfiguration))
                    return;

                this.DisposeMqttClient();
                this.configuration = newConfiguration;
                this.InitializeFromConfig();
            }
            this.Start();
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_serviceStarted || _startInitialized)
                    return;

                _startInitialized = true;

                try
                {
                    EstablishConnection();
                    this._mqttClient!.Subscribe(new string[] { configuration.ResponseTopicName, configuration.TelemetryTopicName }, new byte[] { _QosLevel, _QosLevel });
                    this._serviceStarted = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unbale to start mqtt connection", e.ToString());
                    this._serviceStarted = false;
                    _startInitialized = false;
                }
            }
        }

        public void PublishRequest(string request)
            => PublishMessage(configuration.RequestTopicName, request);

        public void PublishMessage(string topic, string request)
        {
            var bytes = this.ConvertToBytes(request);
            _mqttClient!.Publish(topic, bytes, _QosLevel, false);
        }

        protected virtual byte[] ConvertToBytes<T>(T message)
        {
            if (message is string stringData)
            {
                var jsonObject = JsonConvert.DeserializeObject(stringData, _jsonSerializerSettings);
                var jsonString = JsonConvert.SerializeObject(jsonObject, _jsonSerializerSettings);
                return Encoding.UTF8.GetBytes(jsonString);
            }

            var serializedMessage = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(serializedMessage);
        }

        private void InitializeFromConfig()
        {
            this._mqttClient = new MqttClient(configuration.BrokkerAddress);
            _mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
        }

        private void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.Message);

            if (e.Topic.Equals(this.configuration.ResponseTopicName))
            {
                publishResponseMessage(e);
                return;
            }

            if (e.Topic.Equals(this.configuration.TelemetryTopicName))
            {
                publishTelemetryMessage(e);
                return;
            }

            Console.WriteLine($"Unknon Topic: '{e.Topic}' | Value: {payload}");
        }

        private void publishResponseMessage(MqttMsgPublishEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.Message);
            var responseMessage = JsonConvert.DeserializeObject<ResponseMessage>(payload, _jsonSerializerSettings);
            this.OnResponseMessage?.Invoke(this, responseMessage);
        }

        private void publishTelemetryMessage(MqttMsgPublishEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.Message);
            var telemetryMessage = JsonConvert.DeserializeObject<TelemetryMessage>(payload, _jsonSerializerSettings);
            this.OnTelemetryMessage?.Invoke(this, telemetryMessage);
        }

        private void EstablishConnection()
        {
            if (_mqttClient!.IsConnected)
                return;

            var returnCode = _mqttClient.Connect(configuration.ClientId);

            if (returnCode == MqttMsgConnack.CONN_ACCEPTED)
                return;

            var reason = CodeToText(returnCode);
            throw new Exception(reason);
        }

        private string CodeToText(byte returnCode)
        {
            return returnCode switch
            {
                MqttMsgConnack.CONN_ACCEPTED => "Accepted",
                MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED => "Ident rejected",
                MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED => "Not authorized",
                MqttMsgConnack.CONN_REFUSED_PROT_VERS => "Protocol version",
                MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE => "Server unavailable",
                MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD => "Username/Password",
                _ => $"Unknown return code {returnCode}",
            };
        }

        public void Dispose()
        {
            DisposeMqttClient();
        }

        public void Clear()
        {
            if (OnResponseMessage is not null)
            {
                var responseHandlers = OnResponseMessage.GetInvocationList();
                foreach (var d in responseHandlers)
                    OnResponseMessage -= (d as EventHandler<ResponseMessage>);
            }

            if (OnTelemetryMessage is not null)
            {
                var telemetryHandlers = OnTelemetryMessage.GetInvocationList();
                foreach (var d in telemetryHandlers)
                    OnTelemetryMessage -= (d as EventHandler<TelemetryMessage>);
            }
        }

        private void DisposeMqttClient()
        {
            if (_mqttClient is null)
                return;

            Clear();
            try
            {
                _mqttClient.Unsubscribe(new string[] { configuration.ResponseTopicName, configuration.TelemetryTopicName });
                _mqttClient.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            _serviceStarted = false;
            _startInitialized = false;
        }

        private bool AreTheSame(ConfigurationModel currentConfiguration, ConfigurationModel newConfiguration)
            =>
            currentConfiguration.BrokkerAddress.Equals(newConfiguration.BrokkerAddress, StringComparison.OrdinalIgnoreCase)
            &&
            currentConfiguration.BrokkerPort.Equals(newConfiguration.BrokkerPort)
            &&
            currentConfiguration.ClientId.Equals(newConfiguration.ClientId, StringComparison.OrdinalIgnoreCase)
            &&
            currentConfiguration.RequestTopicName.Equals(newConfiguration.RequestTopicName, StringComparison.OrdinalIgnoreCase)
            &&
            currentConfiguration.ResponseTopicName.Equals(newConfiguration.ResponseTopicName, StringComparison.OrdinalIgnoreCase)
            &&
            currentConfiguration.TelemetryTopicName.Equals(newConfiguration.TelemetryTopicName, StringComparison.OrdinalIgnoreCase);
    }
}
