using System;
using System.Linq;
using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Serialization
{
    public class SerializerFactory : ISerializerFactory
    {
        public static readonly Type[] BuiltInType = new[]
        {
            typeof(bool),
            typeof(byte),
            typeof(char),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(Guid),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(short),
            typeof(string),
            typeof(TimeSpan),
            typeof(uint),
            typeof(ulong),
            typeof(ushort)
        };

        public IDeserializer<T> GetDeserializer<T>()
            => new KafkaJsonSerializer<T>();

        public ISerializer<T> GetSeserializer<T>()
            => new KafkaJsonSerializer<T>();

        private IDeserializer<T> GetSerializer<T>()
        {
            if (IsBuiltInType<T>())
                return null;

            return new KafkaJsonSerializer<T>();
        }

        private static bool IsBuiltInType<T>()
            => BuiltInType.Contains(typeof(T));
    }
}