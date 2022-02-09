using MediatR;

namespace OMP.Connector.Domain.API.Subscriptions.Subscribing
{
    public record SubscribeToNodesRequest : IRequest<SubscribeToNodesResponse>
    {
        public List<MonitoredItem> MonitoredItems { get; set; } = new List<MonitoredItem>();
    }
}