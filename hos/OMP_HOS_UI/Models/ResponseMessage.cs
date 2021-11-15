namespace OmpHandsOnUi
{
    //public class ResponseMessage
    //{
    //    public string? RequestId { get; set; }
    //    public string? Message { get; set; }
    //    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    //    public object? Payload { get; set; }
    //}


    public class ResponseMessage
    {
        public string Namespace { get; set; }
        public Payload Payload { get; set; }
        public string Id { get; set; }
        public string Schema { get; set; }
        public Metadata Metadata { get; set; }
    }

    public class Payload
    {
        public string ResponseStatus { get; set; }
        public Responsesource ResponseSource { get; set; }
        public Respons[] Responses { get; set; }
    }

    public class Responsesource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public string EndpointUrl { get; set; }
    }

    public class Respons
    {
        public string Message { get; set; }
        public string CommandType { get; set; }
    }

    public class Metadata
    {
        public DateTime Timestamp { get; set; }
        public object[] CorrelationIds { get; set; }
        public Destinationidentifier[] DestinationIdentifiers { get; set; }
        public Senderidentifier SenderIdentifier { get; set; }
    }

    public class Destinationidentifier
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Route { get; set; }
    }

}
