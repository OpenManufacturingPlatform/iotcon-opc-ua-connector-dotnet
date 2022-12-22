using MediatR;

namespace OMP.Connector.Domain.API.Discovery.Server
{
    public record ServerDiscoveryRequest : RequestBase, IRequest<ServerDiscoveryResponse> { }

    public record ServerDiscoveryResponse : ResponseBase { }
}