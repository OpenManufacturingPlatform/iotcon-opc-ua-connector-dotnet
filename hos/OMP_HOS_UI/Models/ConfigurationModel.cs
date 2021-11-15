namespace OmpHandsOnUi
{
    public record ConfigurationModel
    {
        public const string RequestTopic = "commands";
        public const string ResponseTopic = "responses";
        public const string TelemetryTopic = "telemetry";

        public string ClientId { get; set; } = Guid.NewGuid().ToString();
        public string BrokkerAddress { get; set; } = "localhost";
        public int BrokkerPort { get; set; } = 1883;
        public string RequestTopicName { get; set; } = RequestTopic;
        public string ResponseTopicName { get; set; } = ResponseTopic;
        public string TelemetryTopicName { get; set; } = TelemetryTopic;
    }
}
