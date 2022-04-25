// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using uPLibrary.Networking.M2Mqtt;

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public record MqttClientSettings
    {
        public string BrokerAddress { get; set; }
        public int BrokerPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public bool Secure { get; set; }
        public string CaCertData { get; set; }
        public string ClientCaCertData { get; set; }
        public bool IgnoreCertificateValidation { get; set; }
        public bool CleanSession { get; set; } = true;
        public bool WillFlag { get; set; } = false;
        public byte WillQosLevel { get; set; } = 0;
        public string WillTopic { get; set; } = null;
        public string WillMessage { get; set; } = null;
        public bool WillRetain { get; set; } = false;
        public ushort KeepAlivePeriod { get; set; } = 60;
        public ushort AutoReconnectTimeInSeconds { get; set; } = 10;
        public MqttTopic[] Topics { get; set; }
        public MqttSslProtocols SslProtocols { get; set; } = MqttSslProtocols.None;

        public MqttProtocolVersion ProtocolVersion { get; set; } = MqttProtocolVersion.Version_3_1;

        public MqttClientSettings() { }

        public MqttClientSettings(
            string brokerAddress,
            int brokerPort,
            string username,
            string password,
            string clientId = null,
            bool secure = false,
            string caCertData = null,
            bool ignoreCertificateValidation = false,
            bool willRetain = false,
            byte willQosLevel = 0,
            bool willFlag = false,
            string willTopic = null,
            string willMessage = null,
            bool cleanSession = false,
            ushort keepAlivePeriod = 60,
            ushort autoReconnectTimeInSeconds = 10)
        {
            BrokerAddress = brokerAddress;
            BrokerPort = brokerPort;
            ClientId = clientId ?? CreateUniqueId();
            Username = username;
            Password = password;
            Secure = secure;
            CaCertData = caCertData;
            IgnoreCertificateValidation = ignoreCertificateValidation;
            WillRetain = willRetain;
            WillQosLevel = willQosLevel;
            WillFlag = willFlag;
            WillTopic = willTopic;
            WillMessage = willMessage;
            CleanSession = cleanSession;
            KeepAlivePeriod = keepAlivePeriod;
            AutoReconnectTimeInSeconds = autoReconnectTimeInSeconds;

        }

        private static string CreateUniqueId()
            => "_" + Guid.NewGuid().ToString().Substring(0, 8);
    }

    public record  MqttTopic
    {
        public string TopicName { get; set; }
        public byte QosLevel { get; set; }
        public bool Retain { get; set; }
    }
}