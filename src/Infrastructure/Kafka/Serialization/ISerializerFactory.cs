using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Serialization
{
    public interface ISerializerFactory
    {
        IDeserializer<T> GetDeserializer<T>();
        public ISerializer<T> GetSeserializer<T>();
    }
}