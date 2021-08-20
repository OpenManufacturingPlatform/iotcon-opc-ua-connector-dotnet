using System.Threading.Tasks;
using Omp.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Application.Services
{
    public interface ICommandService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest);
    }
}