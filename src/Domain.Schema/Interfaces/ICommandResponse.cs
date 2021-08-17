using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Converters;

namespace Omp.Connector.Domain.Schema.Interfaces
{
    [JsonConverter(typeof(CommandResponseConverter))]
    public interface ICommandResponse { }
}