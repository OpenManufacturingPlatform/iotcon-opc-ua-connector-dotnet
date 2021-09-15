namespace OMP.Connector.Infrastructure.MQTT.Serialization
{
    public interface ISerializer
    {
        string Serialize(object value);
        T Deserialize<T>(string value);
    }
}
