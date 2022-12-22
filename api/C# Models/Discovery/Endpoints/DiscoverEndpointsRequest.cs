using MediatR;

namespace OMP.Connector.Domain.API.Discovery.Endpoints
{
    public record DiscoverEndpointsRequest : RequestBase, IRequest<DiscoverEndpointsResponse>
    { }
}