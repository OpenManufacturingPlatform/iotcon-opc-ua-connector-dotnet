using System.Threading.Tasks;
using Omp.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest subscriptionRequests);
    }
}