namespace OMP.Connector.Domain.API.Subscriptions.Subscribing
{
    public record MonitoredItem
    {
        public string NodeId { get; set; } = string.Empty;
        public int SamplingIntervalInMillisecond { get; set; } = 1000;
        public int PublishingIntervalInMillisecond { get; set; } = 2000;
    }

}