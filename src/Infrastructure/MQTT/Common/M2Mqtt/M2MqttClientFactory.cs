using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.MQTT.Extensions;
using uPLibrary.Networking.M2Mqtt;

namespace OMP.Connector.Infrastructure.MQTT.Common.M2Mqtt
{
    public class M2MqttClientFactory: IMqttClientFactory
    {
        private static ILogger<M2MqttClient> _logger;
        private readonly IDictionary<string, M2MqttClient> _mqttClientDictionary;

        public M2MqttClientFactory(ILogger<M2MqttClient> logger)
        {
            _logger = logger;
            this._mqttClientDictionary = new ConcurrentDictionary<string, M2MqttClient>();
        }

        public IMqttClient CreateClient(CommunicationChannelConfiguration channelConfiguration, SharedConfiguration? sharedConfiguration)
        {
            var mqttClientSettings = channelConfiguration.GetConfig<MqttClientSettings>();

            if (this._mqttClientDictionary.ContainsKey(mqttClientSettings.ClientId))
                return this._mqttClientDictionary[mqttClientSettings.ClientId];

            if (mqttClientSettings.SslProtocols == MqttSslProtocols.None && mqttClientSettings.Secure)
                mqttClientSettings.SslProtocols = MqttSslProtocols.TLSv1_2;

            var sslCertificate = mqttClientSettings.Secure ? new X509Certificate(mqttClientSettings.CaCertData.ToByteArray()) : null;
            var clientSslCertificate = !string.IsNullOrWhiteSpace(mqttClientSettings.ClientCaCertData) ? new X509Certificate(mqttClientSettings.ClientCaCertData.ToByteArray()) : null;
            
            var mqttClient = new M2MqttClient(
                mqttClientSettings.BrokerAddress,
                mqttClientSettings.BrokerPort,
                mqttClientSettings.Secure,
                sslCertificate,
                clientSslCertificate,
                mqttClientSettings.SslProtocols,
                mqttClientSettings.IgnoreCertificateValidation ? UserCertificateValidationCallback : (RemoteCertificateValidationCallback)null)
            {
                ProtocolVersion = mqttClientSettings.ProtocolVersion,
            };

            return mqttClient;
        }


        private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                _logger.LogWarning(
                    $"ssl certificate validation failed. Reason: {sslPolicyErrors}. {certificate.Subject} {certificate.Issuer}");

                var chainElementStatus = string.Join(";",
                    chain.ChainStatus.Select(a => string.Concat(a.Status, " ", a.StatusInformation)));
                _logger.LogWarning(chainElementStatus);
            }

            return true;
        }
    }
}
