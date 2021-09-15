namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses
{
    public class PublishPartialSucceeded
    {
        public string Message { get; } = "Partially succeeded";
        public PublishPartialSucceeded()
        { }

        public PublishPartialSucceeded(string message)
        {
            Message = message;
        }
    }
}
