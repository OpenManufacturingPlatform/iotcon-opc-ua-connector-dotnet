namespace OMP.Connector.Domain.API.Subscriptions.Subscribing
{
    public record CreatedMonitoredItem
    {
        public string NodeId { get; set; } = string.Empty;
        public Status Status { get; set; } = new Status();
    }

}