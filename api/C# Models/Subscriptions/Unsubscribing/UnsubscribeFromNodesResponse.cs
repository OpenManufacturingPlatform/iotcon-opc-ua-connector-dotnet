namespace OMP.Connector.Domain.API.Subscriptions.Unsubscribing
{
    public record UnsubscribeFromNodesResponse : ResponseBase
    {
        public List<RemovedMonitoredItem> MonitoredItems { get; set; } = new List<RemovedMonitoredItem>();
    }

}