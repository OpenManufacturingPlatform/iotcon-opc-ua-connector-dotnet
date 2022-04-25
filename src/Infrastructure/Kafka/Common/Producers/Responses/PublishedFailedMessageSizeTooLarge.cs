// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses
{
    public class PublishedFailedMessageSizeTooLarge : PublishFailed
    {
        public PublishedFailedMessageSizeTooLarge(string error)
            : base(error) { }

        public PublishedFailedMessageSizeTooLarge(Exception ex)
            : base(ex) { }
    }
}
