namespace OMP.Connector.Domain.API.Subscriptions.Subscribing
{
    public record SubscribeToNodesResponse : ResponseBase
    {
        public List<CreatedMonitoredItem> MonitoredItems { get; set; } = new List<CreatedMonitoredItem>();
    }

}