using MediatR;

namespace OMP.Connector.Domain.API.Reads.ReadNode
{
    public record ReadNodesRequest : RequestBase, IRequest<ReadNodesResponse>
    {
        public List<string> Nodes { get; set; } = new List<string>();
    }
}