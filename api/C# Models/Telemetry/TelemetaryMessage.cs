namespace OMP.Connector.Domain.API.Telemetry
{
    public record TelemetaryMessage
    {
        public string ConnectorId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string MessageType { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;

        public TelemetaryData Data { get; set; } = new TelemetaryData();
    }

    public record TelemetaryData
    {
        public object Value { get; set; } = new object();
        public string StatusCode { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public long SequenceNr { get; set; }
        public DateTime? SourceTimestamp { get; set; }
        public DateTime? ServerTimestamp { get; set; }
    }
}
