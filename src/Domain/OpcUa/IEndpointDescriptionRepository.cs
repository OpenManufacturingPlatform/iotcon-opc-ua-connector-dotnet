using OMP.Connector.Domain.Models;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IEndpointDescriptionRepository
    {
        bool Add(EndpointDescriptionDto description);

        EndpointDescriptionDto GetByEndpointUrl(string getBaseEndpointUrl);
    }
}