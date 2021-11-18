namespace OmpHandsOnUi
{
	public class TelemetryMessage
    {
        public string Namespace { get; set; }
        public TelemetryPayload Payload { get; set; }
        public string Id { get; set; }
        public string Schema { get; set; }
        public Metadata Metadata { get; set; }
    }
}