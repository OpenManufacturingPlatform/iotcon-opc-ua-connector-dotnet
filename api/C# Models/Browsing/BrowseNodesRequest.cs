using MediatR;

namespace OMP.Connector.Domain.API.Browsing
{
    public record BrowseNodesRequest : RequestBase, IRequest<BrowseNodesResponse>
    {
        public string StartNodeId { get; set; } = string.Empty;
        public int BrowseDepth { get; set; } = 1;
    }
}