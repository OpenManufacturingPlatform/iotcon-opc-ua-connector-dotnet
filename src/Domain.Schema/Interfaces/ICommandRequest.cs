using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Converters;

namespace Omp.Connector.Domain.Schema.Interfaces
{
    [JsonConverter(typeof(CommandRequestConverter))]
    public interface ICommandRequest { }
}