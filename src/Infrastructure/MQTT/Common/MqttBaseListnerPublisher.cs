// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OMP.Connector.Infrastructure.MQTT.Serialization;

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public abstract class MqttBaseListnerPublisher<TClient, TSettings>
        where TClient : IMqttClient
        where TSettings : MqttClientSettings
    {
        #region [Declerations]

        #region [Events]
        public event EventHandler<ErrorEventArgs> OnErrorOccured;
        #endregion

        #region [Fields]
        protected readonly object _lockObject;
        protected readonly ILogger Logger;
        protected readonly TClient Client;
        protected readonly TSettings MqttClientSettings;
        protected readonly ISerializer Serializer;
        protected readonly Timer ReconnectTimer;
        protected bool ShouldBeConnected;
        protected int AutoReconnectTime;
        #endregion
        #endregion

        #region [Ctor]

        protected MqttBaseListnerPublisher(
            TClient client, 
            TSettings mqttClientSettings,
            ISerializer serializer,
            ILogger logger = null, 
            int autoReconnectTimeInSeconds = Constants.ReconnectTimeInSeconds)
        {
            if (string.IsNullOrWhiteSpace(mqttClientSettings.BrokerAddress))
                throw new ArgumentException("Pointless use of MQTT source with no broker address.");

            this.Logger = logger;
            this.MqttClientSettings = mqttClientSettings;
            this.Serializer = serializer;
            this.ReconnectTimer = new Timer(this.DoReconnect, null, Timeout.Infinite, Timeout.Infinite);
            this.Client = client;
            this.Client.ClosedConnection += this.OnClientConnectionClosed;
            this.AutoReconnectTime = autoReconnectTimeInSeconds;
            this._lockObject = new object();
        }

        #endregion

        #region [Public]

        public bool IsUpAndRunning()
            => this.Client?.IsConnected == true;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region [Protected]
        protected virtual Task PublishAsync<T>(T message)
        {
            lock (this._lockObject)
            {
                if (!this.IsUpAndRunning() && !this.EstablishConnection())
                    this.StartAutoReconnect(this.AutoReconnectTime);
            }

            var bytes = this.ConvertToBytes(message);
            foreach (var topic in this.MqttClientSettings.Topics)
                this.Client.Publish(topic.TopicName, bytes, topic.QosLevel, false);

            return Task.CompletedTask;
        }

        protected virtual byte[] ConvertToBytes<T>(T message)
        {
            var serializedMessage = Serializer.Serialize(message);
            return Encoding.UTF8.GetBytes(serializedMessage);
        }

        protected virtual void Dispose(bool disposeAllResources)
        {
            this.ShouldBeConnected = false;
            this.ReconnectTimer.Dispose();

            if (!disposeAllResources)
                return;

            if (this.IsUpAndRunning())
                this.Client.Disconnect();

            SpinWait.SpinUntil(() => this.IsUpAndRunning() == false, TimeSpan.FromSeconds(5));
        }

        protected void RaiseOnErrorOccurred(object sender, Exception exception)
        {
            this.OnErrorOccured?.Invoke(sender, new ErrorEventArgs(exception));
        }
        
        protected virtual bool EstablishConnection()
        {
            this.ShouldBeConnected = true;
            if (this.Client.IsConnected)
                return true;
            try
            {
                this.Logger?.LogInformation("Connecting to {brokerAddress} as {clientId}...", this.MqttClientSettings.BrokerAddress, this.MqttClientSettings.ClientId);

                var returnCode = this.Client.Connect(
                    this.MqttClientSettings.ClientId,
                    this.MqttClientSettings.Username,
                    this.MqttClientSettings.Password,
                    this.MqttClientSettings.WillRetain,
                    this.MqttClientSettings.WillQosLevel,
                    this.MqttClientSettings.WillFlag,
                    this.MqttClientSettings.WillTopic,
                    this.MqttClientSettings.WillMessage,
                    this.MqttClientSettings.CleanSession,
                    this.MqttClientSettings.KeepAlivePeriod);

                if (this.Client.IsConnectionAccepted(returnCode))
                {
                    this.Logger?.LogInformation("Successfully Connected to {brokerAddress} as {clientId}...", this.MqttClientSettings.BrokerAddress, this.MqttClientSettings.ClientId);
                    return true;
                }

                var reason = this.Client.CodeToText(returnCode);
                this.Logger?.LogError("Could not connect to {brokerAddress} as {clientId}. Reason: {reason}", this.MqttClientSettings.BrokerAddress, this.MqttClientSettings.ClientId, reason);
                return false;
                
            }
            catch (Exception e)
            {
                this.Logger?.LogError(e, "{exception} occurred {BrokerAddress}", e.GetType().Name, this.MqttClientSettings.BrokerAddress);
                this.RaiseOnErrorOccurred(this, e);
                return false;
            }
        }

        protected virtual void DropConnection()
        {
            this.Logger?.LogInformation("Closing connection to {brokerAddress}", this.MqttClientSettings.BrokerAddress);
            this.ShouldBeConnected = false;
            this.StopAutoReconnect();

            if (!this.IsUpAndRunning())
                return;

            this.Client.Disconnect();
        }

        protected void OnClientConnectionClosed(object sender, EventArgs e)
        {
            if (!this.ShouldBeConnected)
                return;

            this.Logger?.LogWarning("{brokerAddress} connection lost, will automatically reconnect.", this.MqttClientSettings.BrokerAddress);
            this.StartAutoReconnect();
        }

        protected void StartAutoReconnect(int delaySeconds = 0)
            => this.ReconnectTimer.Change(TimeSpan.FromSeconds(delaySeconds), TimeSpan.FromSeconds(60));

        protected void StopAutoReconnect()
            => this.ReconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);

        protected void DoReconnect(object state)
        {
            if (this.EstablishConnection())
                this.StopAutoReconnect();
        }
        #endregion
    }
}
