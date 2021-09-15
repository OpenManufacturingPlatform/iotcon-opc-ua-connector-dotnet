namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public class MqttMessageEventArgs
    {
        public string Topic { get; internal set; }

        public byte[] Message { get; internal set; }

        public bool DupFlag { get; internal set; }

        public byte QosLevel { get; internal set; }

        public bool Retain { get; internal set; }
    }
}
