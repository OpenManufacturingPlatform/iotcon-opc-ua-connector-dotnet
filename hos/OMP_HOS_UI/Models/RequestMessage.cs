namespace OmpHandsOnUi
{
    public class RequestMessage
    {
        public string RequestId { get; } = Guid.NewGuid().ToString();
        public string? Message { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public object? Payload { get; set; }
    }
}
