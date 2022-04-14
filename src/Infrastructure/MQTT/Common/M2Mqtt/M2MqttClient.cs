// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using OMP.Connector.Infrastructure.MQTT.Common.Events;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace OMP.Connector.Infrastructure.MQTT.Common
{

    public class M2MqttClient : MqttClient, IMqttClient
    {
        public event ClosedConnectionEventHandler ClosedConnection;
        public event OnMessageEventHandler OnMessageReceived;

        public M2MqttClient(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol,
            RemoteCertificateValidationCallback userCertificateValidationCallback)
            : base(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback)
        {
            this.ConnectionClosed += M2MqttClient_ConnectionClosed;
            this.MqttMsgPublishReceived += M2MqttClient_MqttMsgPublishReceived;
        }

        private void M2MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var mqqtMessage = new MqttMessageEventArgs
            {
                DupFlag = e.DupFlag,
                Message = e.Message,
                QosLevel = e.QosLevel,
                Retain = e.Retain,
                Topic = e.Topic
            };

            this.OnMessageReceived?.Invoke(sender, mqqtMessage);
        }

        private void M2MqttClient_ConnectionClosed(object sender, EventArgs e)
        => this.ClosedConnection?.Invoke(sender, e);

        public string CodeToText(byte returnCode)
        {
            switch (returnCode)
            {
                case MqttMsgConnack.CONN_ACCEPTED: return "Accepted";
                case MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED: return "Ident rejected";
                case MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED: return "Not authorized";
                case MqttMsgConnack.CONN_REFUSED_PROT_VERS: return "Protocol version";
                case MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE: return "Server unavailable";
                case MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD: return "Username/Password";
                default: return $"Unknown return code {returnCode}";
            }
        }

        public bool IsConnectionAccepted(byte returnCode)
            => returnCode == MqttMsgConnack.CONN_ACCEPTED;

    }
}