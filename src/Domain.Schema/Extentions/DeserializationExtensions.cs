using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.Extentions
{
    public static class DeserializationExtensions
    {
        public static T Deserialize<T>(this string data)
            => JsonConvert.DeserializeObject<T>(data);
    }
}