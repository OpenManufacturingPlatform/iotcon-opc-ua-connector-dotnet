using MediatR;

namespace OMP.Connector.Domain.API.Calls
{
    public record CallRequest : RequestBase, IRequest<CallResponse>
    {
        public List<NodeInCallRequest> Nodes { get; set; } = new List<NodeInCallRequest>();
    }
}