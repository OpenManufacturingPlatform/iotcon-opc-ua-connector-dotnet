using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OMP.Connector.Infrastructure.MQTT.Extensions
{
    public static class ObjectExtensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;

            var binaryFormatter = new BinaryFormatter();
            using var memoryStream = new MemoryStream();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            binaryFormatter.Serialize(memoryStream, obj);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            return memoryStream.ToArray();
        }
    }
}
