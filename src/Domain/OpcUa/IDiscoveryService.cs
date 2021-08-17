using System.Threading.Tasks;
using Omp.Connector.Domain.Schema.Messages;
using Omp.Connector.Domain.Schema.Responses.Discovery;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IDiscoveryService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest);
        Task<ServerDiscoveryResponse> ExecuteAsync(string endpointUrl);
    }
}