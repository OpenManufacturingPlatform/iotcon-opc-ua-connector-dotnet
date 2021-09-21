namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public class Constants
    {
        public const int ReconnectTimeInSeconds = 10;
        public const byte QOS_LEVEL_AT_MOST_ONCE = 0;
        public const byte QOS_LEVEL_AT_LEAST_ONCE = 1;
        public const byte QOS_LEVEL_EXACTLY_ONCE = 2;
        public const byte QOS_LEVEL_GRANTED_FAILURE = 128;
    }
}
