using MediatR;

namespace OMP.Connector.Domain.API
{
    public record WriteValuesRequest : RequestBase, IRequest<WriteValuesResponse>
    {
        public List<NodeWriteValue> Nodes { get; set; } = new List<NodeWriteValue>();
    }
}