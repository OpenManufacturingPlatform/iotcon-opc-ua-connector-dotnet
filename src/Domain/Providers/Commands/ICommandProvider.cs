using System.Collections.Generic;
using System.Threading.Tasks;
using Omp.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Providers.Commands
{
    public interface ICommandProvider
    {
        Task<IEnumerable<ICommandResponse>> ExecuteAsync();
    }
}