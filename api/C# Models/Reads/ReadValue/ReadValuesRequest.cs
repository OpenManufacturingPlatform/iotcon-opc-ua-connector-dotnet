using MediatR;

namespace OMP.Connector.Domain.API.Reads.ReadValue
{
    public record ReadValuesRequest : RequestBase, IRequest<ReadValuesResponse>
    {
        public List<string> Nodes { get; set; } = new List<string>();
    }
}