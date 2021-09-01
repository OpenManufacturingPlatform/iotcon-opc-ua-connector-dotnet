using System;

namespace OMP.Device.Connector.Kafka.Common.Producers.Responses
{
    public class PublishedFailedMessageSizeTooLarge : PublishFailed
    {
        public PublishedFailedMessageSizeTooLarge(string error)
            : base(error) { }

        public PublishedFailedMessageSizeTooLarge(Exception ex)
            : base(ex) { }
    }
}
