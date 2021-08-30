using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Extenions
{
    public static class DeserializationExtensions
    {
        public static T Deserialize<T>(this string data)
            => JsonConvert.DeserializeObject<T>(data);
    }
}