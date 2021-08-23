using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Serialization
{
    public interface ISerializerFactory
    {
        IDeserializer<T> GetDeserializer<T>();
        public ISerializer<T> GetSeserializer<T>();
    }
}