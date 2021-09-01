using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Responses.Discovery;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IDiscoveryService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest);
        Task<ServerDiscoveryResponse> ExecuteAsync(string endpointUrl);
    }
}