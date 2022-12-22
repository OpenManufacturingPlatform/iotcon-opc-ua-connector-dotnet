using MediatR;

namespace OMP.Connector.Domain.API.Subscriptions.Unsubscribing
{
    public record UnsubscribeFromNodesRequest : RequestBase, IRequest<UnsubscribeFromNodesResponse>
    {
        public List<string> Nodes { get; set; } = new List<string>();
    }
}