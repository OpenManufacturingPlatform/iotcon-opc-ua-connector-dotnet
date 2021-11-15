namespace OmpHandsOnUi
{
    //public class TelemetryMessage
    //{
    //    public string? Message { get; set; }
    //    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    //    public object? Payload { get; set; }
    //}


    public class TelemetryMessage
    {
        public string Namespace { get; set; }
        public TelemetryPayload Payload { get; set; }
        public string Id { get; set; }
        public string Schema { get; set; }
        public Metadata Metadata { get; set; }
    }

    public class TelemetryPayload
    {
        public Datasource DataSource { get; set; }
        public Data Data { get; set; }
    }

    public class Datasource
    {
        public string EndpointUrl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
    }

    public class Data
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
        public DateTime LastChangeTimestamp { get; set; }
        public DateTime MeasurementTimestamp { get; set; }
        public string DataType { get; set; }
    }

    //public class Metadata
    //{
    //    public DateTime timestamp { get; set; }
    //    public object[] correlationIds { get; set; }
    //    public Senderidentifier senderIdentifier { get; set; }
    //    public object[] destinationIdentifiers { get; set; }
    //}

    public class Senderidentifier
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string route { get; set; }
    }

}
