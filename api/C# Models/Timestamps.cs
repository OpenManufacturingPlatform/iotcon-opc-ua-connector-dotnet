namespace OMP.Connector.Domain.API
{
    public record Timestamps
    {
        public DateTime ReceivedTime { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}