using System.Collections.Generic;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers.Commands;
using Omp.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Application.Factories
{
    public interface ICommandProviderFactory
    {
        IEnumerable<ICommandProvider> GetProcessors(IEnumerable<ICommandRequest> nodeCommands, IOpcSession opcSession);
    }
}