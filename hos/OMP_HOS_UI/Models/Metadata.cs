namespace OmpHandsOnUi
{
	public class Metadata
    {
        public DateTime Timestamp { get; set; }
        public object[] CorrelationIds { get; set; }
        public Destinationidentifier[] DestinationIdentifiers { get; set; }
        public Senderidentifier SenderIdentifier { get; set; }
    }

}
