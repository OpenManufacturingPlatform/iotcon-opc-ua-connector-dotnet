using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OMP.Device.Connector.Kafka.Serialization
{
    public class JsonSerializerFactory: ISerializerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public JsonSerializerFactory(ILoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
        }
        public IDeserializer<T> GetDeserializer<T>()
            => new KafkaJsonSerializer<T>();

        public ISerializer<T> GetSeserializer<T>()
            => new KafkaJsonSerializer<T>();
    }
}