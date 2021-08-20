using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Application.Services
{
    public interface ICommandService
    {
        Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest);
    }
}