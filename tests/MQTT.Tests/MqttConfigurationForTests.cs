// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.MQTT.Common;
using System;

namespace MQTT.Tests
{
    internal class MqttConfigurationForTests : CommunicationChannelConfiguration
    {
        internal MqttClientSettings? MqttClientSettings { get; set; }

        public MqttConfigurationForTests()
        {
            InitializeMqttSettings();
        }

        public MqttConfigurationForTests(MqttClientSettings mqttClientSettings)
        {
            MqttClientSettings = mqttClientSettings;
        }

        public override T GetConfig<T>()
        {
            var config = new T();
            if (typeof(MqttClientSettings) == typeof(T))
            {
                if (this.MqttClientSettings is null)
                    InitializeMqttSettings();

                var compInfoPackage = config as MqttClientSettings;
                compInfoPackage!.BrokerAddress = this.MqttClientSettings!.BrokerAddress;
                compInfoPackage!.BrokerPort = this.MqttClientSettings.BrokerPort;
                compInfoPackage!.Username = this.MqttClientSettings.Username;
                compInfoPackage!.Password = this.MqttClientSettings.Password;
                compInfoPackage!.ClientId = this.MqttClientSettings.ClientId;
                compInfoPackage!.Secure = this.MqttClientSettings.Secure;
                compInfoPackage!.CaCertData = this.MqttClientSettings.CaCertData;
                compInfoPackage!.ClientCaCertData = this.MqttClientSettings.ClientCaCertData;
                compInfoPackage!.IgnoreCertificateValidation = this.MqttClientSettings.IgnoreCertificateValidation;
                compInfoPackage!.CleanSession = this.MqttClientSettings.CleanSession;
                compInfoPackage!.WillFlag = this.MqttClientSettings.WillFlag;
                compInfoPackage!.WillQosLevel = this.MqttClientSettings.WillQosLevel;
                compInfoPackage!.WillTopic = this.MqttClientSettings.WillTopic;
                compInfoPackage!.WillMessage = this.MqttClientSettings.WillMessage;
                compInfoPackage!.WillRetain = this.MqttClientSettings.WillRetain;
                compInfoPackage!.KeepAlivePeriod = this.MqttClientSettings.KeepAlivePeriod;
                compInfoPackage!.AutoReconnectTimeInSeconds = this.MqttClientSettings.AutoReconnectTimeInSeconds;
                compInfoPackage!.ProtocolVersion = this.MqttClientSettings.ProtocolVersion;

                return config;
            }

            return base.GetConfig<T>();
        }

        private void InitializeMqttSettings()
        {
            this.MqttClientSettings = new MqttClientSettings
            {
                BrokerAddress = "localhost",
                BrokerPort = 1883,
                Username = "",
                Password = "",
                ClientId = Guid.NewGuid().ToString(),
                Secure = false,
                CaCertData = null,
                ClientCaCertData = null,
                IgnoreCertificateValidation = false,
                CleanSession = true,
                WillFlag = false,
                WillQosLevel = 0,
                WillTopic = null,
                WillMessage = null,
                WillRetain = false,
                KeepAlivePeriod = 60,
                AutoReconnectTimeInSeconds = 10,
                SslProtocols = uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None,
                ProtocolVersion = uPLibrary.Networking.M2Mqtt.MqttProtocolVersion.Version_3_1
            };
        }
    }
}