using OMP.Connector.Infrastructure.MQTT.Common.Events;

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public interface IMqttClient
    {
        event ClosedConnectionEventHandler ClosedConnection;
        event OnMessageEventHandler OnMessageReceived;
        bool IsConnected { get; }
        byte Connect(string clientId);
        byte Connect(string clientId, string username, string password);
        byte Connect(string clientId, string username, string password, bool cleanSession, ushort keepAlivePeriod);
        byte Connect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod);
        void Disconnect();
        public bool IsConnectionAccepted(byte returnCode);
        public string CodeToText(byte returnCode);
        ushort Subscribe(string[] topics, byte[] qosLevels);
        ushort Unsubscribe(string[] topics);
        ushort Publish(string topic, byte[] message);
        ushort Publish(string topic, byte[] message, byte qosLevel, bool retain);
    }
}
